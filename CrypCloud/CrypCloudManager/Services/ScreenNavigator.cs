using CrypCloud.Manager.ViewModels;
using System.Collections.Generic;

namespace CrypCloud.Manager.Services
{
    public class ScreenNavigator
    {
        private readonly Dictionary<ScreenPaths, BaseViewModel> screens = new Dictionary<ScreenPaths, BaseViewModel>();

        public void AddScreenWithPath(BaseViewModel baseModel, ScreenPaths path)
        {
            screens.Add(path, baseModel);
            baseModel.Navigator = this;
            baseModel.IsActive = false;
        }

        public void ShowScreenWithPath(ScreenPaths path)
        {
            if (!screens.ContainsKey(path))
            {
                return;
            }

            foreach (KeyValuePair<ScreenPaths, BaseViewModel> screenViewModel in screens)
            {
                screenViewModel.Value.IsActive = false;
            }

            screens[path].IsActive = true;
        }


    }
}
