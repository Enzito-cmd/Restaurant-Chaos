using UnityEngine;

namespace RestaurantChaos.Clients
{
    public class OrderBoardUI : MonoBehaviour
    {
        [Header("Table Slots")]
        [SerializeField] private OrderTicketUI[] slots;

        private void OnEnable()
        {
            OrderBoard.TicketOpened += HandleTicketOpened;
            OrderBoard.TicketClosed += HandleTicketClosed;
        }

        private void OnDisable()
        {
            OrderBoard.TicketOpened -= HandleTicketOpened;
            OrderBoard.TicketClosed -= HandleTicketClosed;
        }

        private void HandleTicketOpened(OrderTicket ticket)
        {
            int index = ticket.TableNumber - 1;

            if (index < 0 || index >= slots.Length) return;
            if (slots[index] == null) return;

            slots[index].Show(ticket.Meal);
        }

        private void HandleTicketClosed(int tableNumber)
        {
            int index = tableNumber - 1;

            if (index < 0 || index >= slots.Length) return;
            if (slots[index] == null) return;

            slots[index].Hide();
        }
    }
}
