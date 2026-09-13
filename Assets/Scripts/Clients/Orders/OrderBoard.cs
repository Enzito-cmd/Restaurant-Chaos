using System;

namespace RestaurantChaos.Clients
{
    public static class OrderBoard
    {
        public static event Action<OrderTicket> TicketOpened;
        public static event Action<int> TicketClosed;

        public static void OpenTicket(int tableNumber, MealDefinition meal, ClientBase owner)
        {
            OrderTicket ticket = new OrderTicket();
            ticket.TableNumber = tableNumber;
            ticket.Meal = meal;
            ticket.Owner = owner;

            if (TicketOpened != null)
            {
                TicketOpened.Invoke(ticket);
            }
        }

        public static void CloseTicket(int tableNumber)
        {
            if (TicketClosed != null)
            {
                TicketClosed.Invoke(tableNumber);
            }
        }
    }
}
