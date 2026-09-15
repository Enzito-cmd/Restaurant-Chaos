using System;
using System.Collections.Generic;

namespace RestaurantChaos.Clients
{
    public static class OrderBoard
    {
        private static readonly Dictionary<int, OrderTicket> openTickets = new Dictionary<int, OrderTicket>();

        public static event Action<OrderTicket> TicketOpened;
        public static event Action<int> TicketClosed;

        public static IEnumerable<OrderTicket> OpenTickets => openTickets.Values;

        public static void OpenTicket(int tableNumber, MealDefinition meal, ClientBase owner)
        {
            OrderTicket ticket = new OrderTicket();
            ticket.TableNumber = tableNumber;
            ticket.Meal = meal;
            ticket.Owner = owner;

            openTickets[tableNumber] = ticket;

            if (TicketOpened != null)
            {
                TicketOpened.Invoke(ticket);
            }
        }

        public static void CloseTicket(int tableNumber)
        {
            openTickets.Remove(tableNumber);

            if (TicketClosed != null)
            {
                TicketClosed.Invoke(tableNumber);
            }
        }

        public static void ClearAll()
        {
            openTickets.Clear();
        }
    }
}
