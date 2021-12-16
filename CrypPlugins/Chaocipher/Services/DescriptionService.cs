using CrypTool.Chaocipher.Enums;
using CrypTool.Chaocipher.Models;
using CrypTool.Chaocipher.Properties;

namespace CrypTool.Chaocipher.Services
{
    public static class DescriptionService
    {
        private static string GetTranslation(this Step step)
        {
            return PresentationTranslation.ResourceManager.GetString(step.ToString());
        }

        public static CipherResult GenerateDescription(this CipherResult result)
        {
            for (int index = 0; index < result.PresentationStates.Count; index++)
            {
                PresentationState presentationState = result.PresentationStates[index];
                presentationState.Description.Index = index;
                presentationState.Description.Text = presentationState.Step.GetTranslation();
            }

            return result;
        }
    }
}