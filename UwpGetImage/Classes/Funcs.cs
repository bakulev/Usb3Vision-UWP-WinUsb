using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UwpGetImage.Classes
{
    static class Funcs
    {
        public static async Task PutTaskDelay(int interval)
        {
            await Task.Delay(interval);
        }
    }
}
