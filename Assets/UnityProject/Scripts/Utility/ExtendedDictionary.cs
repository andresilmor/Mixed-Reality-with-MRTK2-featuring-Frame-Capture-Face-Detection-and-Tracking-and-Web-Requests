using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.InputSystem;
public class ExtendedDictonary<Tkey, Tvalue> : Dictionary<Tkey, Tvalue>
    {
        public Dictionary<Tkey, Tvalue> Items = new Dictionary<Tkey, Tvalue>();
        public ExtendedDictonary() : base() { }
        ExtendedDictonary(int capacity) : base(capacity) { }
        //
        // Do all your implementations here...
        //

        public event EventHandler ValueChanged;

        public void OnValueChanged(Object sender, EventArgs e)
        {
            EventHandler handler = ValueChanged;
            if (null != handler) handler(this, EventArgs.Empty);
        }

        public void AddItem(Tkey key, Tvalue value)
        {
            try
            {
                Items.Add(key, value);
                OnValueChanged(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

    //
    // Similarly Do implementation for Update , Delete and Value Changed checks (Note: Value change can be monitored using a thread)
    //
}
