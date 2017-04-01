using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DRLib.Patterns.Observer
{
    public class Messenger
    {
        Dictionary<ListenerType, List<Listener>> listeners = new Dictionary<ListenerType, List<Listener>>();

        public void Send<TSender, TMessage>(TSender sender, TMessage message)
            where TSender : class
        {
            List<Listener> ls;

            if (listeners.TryGetValue(ListenerType.Create<TSender, TMessage>(), out ls))
                foreach (var l in ls)
                    (l as Listener<TSender, TMessage>).Invoke(sender, message);
        }

        public void AddListener<TSender, TMessage>(Action<TSender, TMessage> listener)
            where TSender : class
        {
            List<Listener> ls;

            var listenerType = ListenerType.Create<TSender, TMessage>();

            if (listeners.TryGetValue(listenerType, out ls))
            {
                var index = IndexOfListener(ls, listener);
                if(index == -1)
                    ls.Add(new Listener<TSender, TMessage>(listener));
            }
            else
            {
                ls = new List<Listener>() { new Listener<TSender, TMessage>(listener) };
                listeners.Add(listenerType, ls);
            }
        }

        public void RemoveListener<TSender, TMessage>(Action<TSender, TMessage> listener)
            where TSender : class
        {
            List<Listener> ls;

            if (listeners.TryGetValue(ListenerType.Create<TSender, TMessage>(), out ls))
            {
                var index = IndexOfListener(ls, listener);
                if (index != -1)
                    ls.RemoveAt(index);
            }
        }

        int IndexOfListener<TSender, TMessage>(List<Listener> liseners, Action<TSender, TMessage> listener)
            where TSender : class
        {
            int i = 0;
            foreach (var l in liseners)
            {
                if (l.Equals(listener))
                    return i;
                ++i;
            }
            return -1;
        }

        struct ListenerType
        {
            Type senderType;
            Type messageType;

            public override bool Equals(object obj)
            {
                ListenerType lt = (ListenerType)obj;
                return senderType == lt.senderType && messageType == lt.messageType;
            }

            public override int GetHashCode()
            {
                return senderType.GetHashCode() + messageType.GetHashCode();
            }

            public static ListenerType Create<TSeneder, TMessage>()
                where TSeneder : class
            {
                return new ListenerType
                {
                    senderType = typeof(TSeneder),
                    messageType = typeof(TMessage)
                };
            }
        }

        abstract class Listener
        {
        }

        class Listener<TSender, TMessage> : Listener
            where TSender : class
        {
            public Action<TSender, TMessage> listener;

            public Listener(Action<TSender, TMessage> listener)
            {
                this.listener = listener;
            }

            public void Invoke(TSender sender, TMessage message)
            {
                listener(sender, message);
            }

            public override bool Equals(object obj)
            {
                return listener.Equals(obj);
            }
            public override int GetHashCode()
            {
                return listener.GetHashCode();
            }
        }
    }
}
