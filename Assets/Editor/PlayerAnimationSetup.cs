using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class PlayerAnimationSetup
{
    private const string Folder = "Assets/3D Assets/Player/RiggedPlayer/";
    private const string CharacterFile = "T-Pose-Character-With-Bones.fbx";
    private const string WalkFile = "Walk.fbx";
    private const string ControllerPath = "Assets/3D Assets/Player/PlayerAnimator.controller";
    private const string MovingParameter = "IsMoving";
    private const float IdleFade = 0.25f;
    private const float WalkFade = 0.15f;

    private static readonly string[] IdleFiles = { "Idle1.fbx", "Idle2.fbx", "Idle3.fbx" };
    private static readonly string[] ExtraFiles = { "WalkingWSomething.fbx", "Interact.fbx" };

    [MenuItem("Restaurant/Setup Player Animations")]
    public static void Run()
    {
        if (!ConfigureCharacter(out ModelImporter characterImporter, out Avatar characterAvatar))
        {
            return;
        }

        bool allValid = true;

        foreach (string file in IdleFiles)
        {
            allValid &= ConfigureAnimationFile(file, true, characterImporter, characterAvatar);
        }

        allValid &= ConfigureAnimationFile(WalkFile, true, characterImporter, characterAvatar);

        foreach (string file in ExtraFiles)
        {
            ConfigureAnimationFile(file, false, characterImporter, characterAvatar);
        }

        if (!allValid)
        {
            Debug.LogError("PlayerAnimationSetup: one or more clips could not be set up, controller was not built. See the messages above.");
            return;
        }

        BuildController();
    }

    private static bool ConfigureCharacter(out ModelImporter importer, out Avatar avatar)
    {
        string path = Folder + CharacterFile;

        importer = AssetImporter.GetAtPath(path) as ModelImporter;
        avatar = null;

        if (importer == null)
        {
            Debug.LogError("PlayerAnimationSetup: character not found at " + path);
            return false;
        }

        importer.animationType = ModelImporterAnimationType.Human;
        importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        importer.SaveAndReimport();

        avatar = LoadAvatar(path);

        if (avatar == null || !avatar.isValid || !avatar.isHuman)
        {
            Debug.LogError("PlayerAnimationSetup: the character did not produce a valid Humanoid Avatar. Select " + CharacterFile + ", Rig tab, Configure..., fix the red bones and Apply, then run this again.");
            return false;
        }

        Debug.Log("PlayerAnimationSetup: character Avatar OK (" + avatar.name + ").");

        return true;
    }

    private static bool ConfigureAnimationFile(string file, bool required, ModelImporter characterImporter, Avatar characterAvatar)
    {
        string path = Folder + file;
        ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;

        if (importer == null)
        {
            if (required)
            {
                Debug.LogError("PlayerAnimationSetup: missing file " + path);
            }

            return !required;
        }

        importer.animationType = ModelImporterAnimationType.Human;

        bool copiesCharacterAvatar = SharesSkeleton(importer, characterImporter);

        if (copiesCharacterAvatar)
        {
            importer.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
            importer.sourceAvatar = characterAvatar;
        }
        else
        {
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            Debug.LogWarning("PlayerAnimationSetup: " + file + " does not share the character's bone names, it gets its own Avatar. If it looks tilted or twisted, download it again from Mixamo using the same uploaded character.");
        }

        importer.SaveAndReimport();

        if (!copiesCharacterAvatar)
        {
            Avatar ownAvatar = LoadAvatar(path);

            if (ownAvatar == null || !ownAvatar.isValid || !ownAvatar.isHuman)
            {
                ReportFailure(required, "PlayerAnimationSetup: " + file + " did not produce a valid Humanoid Avatar. Select it, Rig tab, Configure..., and fix the bone mapping.");
                return false;
            }
        }

        ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;

        if (clips == null || clips.Length == 0)
        {
            Debug.LogError("PlayerAnimationSetup: " + file + " has no animation take.");
            return false;
        }

        ModelImporterClipAnimation clip = clips[0];
        clip.name = Path.GetFileNameWithoutExtension(file);
        clip.loopTime = true;
        clip.lockRootRotation = true;
        clip.lockRootHeightY = true;
        clip.lockRootPositionXZ = true;

        importer.clipAnimations = new ModelImporterClipAnimation[] { clip };
        importer.SaveAndReimport();

        return true;
    }

    private static bool SharesSkeleton(ModelImporter animationImporter, ModelImporter characterImporter)
    {
        HashSet<string> transformNames = new HashSet<string>();

        foreach (string transformPath in animationImporter.transformPaths)
        {
            int separator = transformPath.LastIndexOf('/');
            transformNames.Add(transformPath.Substring(separator + 1));
        }

        foreach (HumanBone bone in characterImporter.humanDescription.human)
        {
            if (!transformNames.Contains(bone.boneName))
            {
                return false;
            }
        }

        return true;
    }

    private static void ReportFailure(bool required, string message)
    {
        if (required)
        {
            Debug.LogError(message);
        }
        else
        {
            Debug.LogWarning(message);
        }
    }

    private static Avatar LoadAvatar(string path)
    {
        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Avatar>().FirstOrDefault();
    }

    private static AnimationClip LoadClip(string file)
    {
        string path = Folder + file;
        string clipName = Path.GetFileNameWithoutExtension(file);

        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().FirstOrDefault(clip => clip.name == clipName);
    }

    private static void BuildController()
    {
        AnimationClip[] idleClips = new AnimationClip[IdleFiles.Length];

        for (int i = 0; i < IdleFiles.Length; i++)
        {
            idleClips[i] = LoadClip(IdleFiles[i]);

            if (idleClips[i] == null)
            {
                Debug.LogError("PlayerAnimationSetup: clip not found in " + IdleFiles[i]);
                return;
            }
        }

        AnimationClip walkClip = LoadClip(WalkFile);

        if (walkClip == null)
        {
            Debug.LogError("PlayerAnimationSetup: clip not found in " + WalkFile);
            return;
        }

        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);

        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        }

        ClearController(controller);

        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        AnimatorState[] idleStates = new AnimatorState[IdleFiles.Length];

        for (int i = 0; i < IdleFiles.Length; i++)
        {
            idleStates[i] = machine.AddState("Idle" + (i + 1), new Vector3(300f, 100f * i, 0f));
            idleStates[i].motion = idleClips[i];
            idleStates[i].writeDefaultValues = true;
        }

        AnimatorState walkState = machine.AddState("Walk", new Vector3(600f, 100f, 0f));
        walkState.motion = walkClip;
        walkState.writeDefaultValues = true;

        machine.defaultState = idleStates[0];

        foreach (AnimatorState idleState in idleStates)
        {
            AnimatorStateTransition toWalk = idleState.AddTransition(walkState);
            toWalk.AddCondition(AnimatorConditionMode.If, 0f, MovingParameter);
            toWalk.hasExitTime = false;
            toWalk.hasFixedDuration = true;
            toWalk.duration = WalkFade;
        }

        for (int i = 0; i < idleStates.Length; i++)
        {
            AnimatorState next = idleStates[(i + 1) % idleStates.Length];
            AnimatorStateTransition cycle = idleStates[i].AddTransition(next);
            cycle.hasExitTime = true;
            cycle.exitTime = Mathf.Max(0f, 1f - IdleFade / idleClips[i].length);
            cycle.hasFixedDuration = true;
            cycle.duration = IdleFade;
            cycle.interruptionSource = TransitionInterruptionSource.Source;
        }

        AnimatorStateTransition toIdle = walkState.AddTransition(idleStates[0]);
        toIdle.AddCondition(AnimatorConditionMode.IfNot, 0f, MovingParameter);
        toIdle.hasExitTime = false;
        toIdle.hasFixedDuration = true;
        toIdle.duration = WalkFade;

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();

        Debug.Log("PlayerAnimationSetup: done. Idle1 " + idleClips[0].length.ToString("F2") + "s, Idle2 " + idleClips[1].length.ToString("F2") + "s, Idle3 " + idleClips[2].length.ToString("F2") + "s, Walk " + walkClip.length.ToString("F2") + "s. Controller: " + ControllerPath);
    }

    private static void ClearController(AnimatorController controller)
    {
        AnimatorStateMachine machine = controller.layers[0].stateMachine;

        foreach (ChildAnimatorState child in machine.states.ToArray())
        {
            machine.RemoveState(child.state);
        }

        foreach (AnimatorControllerParameter parameter in controller.parameters.ToArray())
        {
            controller.RemoveParameter(parameter);
        }

        controller.AddParameter(MovingParameter, AnimatorControllerParameterType.Bool);
    }
}
