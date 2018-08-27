using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using Microsoft.Toolkit.Uwp.Notifications;

namespace UwpGetImage.Models
{
    class ToastNotificationString
    {
        private ToastNotification toast;
        public ToastNotificationString(string title, string content, DateTimeOffset duration)
        {
            ToastVisual visual = new ToastVisual()
            {
                BindingGeneric = new ToastBindingGeneric()
                {
                    Children =
                    {
                        new AdaptiveText()
                        {
                            Text = title
                        },

                        new AdaptiveText()
                        {
                            Text = content
                        }
                    }
                }
            };


            // Now we can construct the final toast content
            ToastContent toastContent = new ToastContent()
            {
                Visual = visual,
            };

            // And create the toast notification
            toast = new ToastNotification(toastContent.GetXml()) {ExpirationTime = duration};

            //set duration
        }

        public void Show()
        {
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
    }
}
