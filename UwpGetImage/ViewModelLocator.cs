using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using UwpGetImage.ViewModels;

namespace UwpGetImage
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<MainPageViewModel>();
        }

        public MainPageViewModel MainPage => ServiceLocator.Current.GetInstance<MainPageViewModel>();
    }
}
