using CrypTool.Chaocipher.Enums;
using CrypTool.Chaocipher.Models;
using CrypTool.Chaocipher.Properties;

namespace CrypTool.Chaocipher.Services
{
    public static class DescriptionService
    {
        private static string GetTranslation(this Step step) =>
            PresentationTranslation.ResourceManager.GetString(step.ToString());

        public static CipherResult GenerateDescription(this CipherResult result)
        {
            for (var index = 0; index < result.PresentationStates.Count; index++)
            {
                var presentationState = result.PresentationStates[index];
                presentationState.Description.Index = index;
                presentationState.Description.Text = presentationState.Step.GetTranslation();
            }

            return result;
        }
    }
}