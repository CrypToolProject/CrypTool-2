using CrypTool.MD5.Algorithm;
using CrypTool.MD5.Presentation.States;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Controls;


namespace CrypTool.MD5.Presentation.Helpers
{
    public class PresentationControlFactory
    {
        private static readonly Dictionary<MD5StateDescription, Type> statePresentationClasses = new Dictionary<MD5StateDescription, Type>();

        private readonly Dictionary<Type, UserControl> instances = new Dictionary<Type, UserControl>();

        static PresentationControlFactory()
        {
            RegisterPresentationClass(MD5StateDescription.UNINITIALIZED, typeof(UninitializedPresentation));
            RegisterPresentationClass(MD5StateDescription.INITIALIZED, typeof(InitializedPresentation));
            RegisterPresentationClass(MD5StateDescription.READING_DATA, typeof(ReadingDataPresentation));
            RegisterPresentationClass(MD5StateDescription.READ_DATA, typeof(ReadDataPresentation));

            RegisterPresentationClass(MD5StateDescription.STARTING_PADDING, typeof(StartingPaddingPresentation));
            RegisterPresentationClass(MD5StateDescription.ADDING_PADDING_BYTES, typeof(AddingPaddingBytesPresentation));
            RegisterPresentationClass(MD5StateDescription.ADDED_PADDING_BYTES, typeof(AddedPaddingBytesPresentation));
            RegisterPresentationClass(MD5StateDescription.ADDING_LENGTH, typeof(AddingLengthPresentation));
            RegisterPresentationClass(MD5StateDescription.ADDED_LENGTH, typeof(AddedLengthPresentation));
            RegisterPresentationClass(MD5StateDescription.FINISHED_PADDING, typeof(FinishedPaddingPresentation));

            RegisterPresentationClass(MD5StateDescription.STARTING_COMPRESSION, typeof(StartingCompressionPresentation));
            RegisterPresentationClass(MD5StateDescription.STARTING_ROUND, typeof(StartingRoundPresentation));
            RegisterPresentationClass(MD5StateDescription.STARTING_ROUND_STEP, typeof(RoundStepPresentation));
            RegisterPresentationClass(MD5StateDescription.FINISHED_ROUND_STEP, typeof(RoundStepPresentation));
            RegisterPresentationClass(MD5StateDescription.FINISHED_ROUND, typeof(FinishedRoundPresentation));
            RegisterPresentationClass(MD5StateDescription.FINISHING_COMPRESSION, typeof(FinishingCompressionPresentation));
            RegisterPresentationClass(MD5StateDescription.FINISHED_COMPRESSION, typeof(FinishedCompressionPresentation));

            RegisterPresentationClass(MD5StateDescription.FINISHED, typeof(FinishedPresentation));
        }

        public static void RegisterPresentationClass(MD5StateDescription stateDescription, Type presentationClass)
        {
            if (!typeof(UserControl).IsAssignableFrom(presentationClass))
            {
                throw new ArgumentException("Registered type must be subclass of UserControl");
            }

            ConstructorInfo defaultConstructor = presentationClass.GetConstructor(new Type[0]);
            if (defaultConstructor == null)
            {
                throw new ArgumentException("Registered type must have default constructor");
            }

            statePresentationClasses.Add(stateDescription, presentationClass);
        }

        public UserControl GetPresentationControlForState(MD5StateDescription stateDescription)
        {
            if (!statePresentationClasses.ContainsKey(stateDescription))
            {
                return null;
            }

            Type controlType = statePresentationClasses[stateDescription];

            if (instances.ContainsKey(controlType))
            {
                return instances[controlType];
            }

            ConstructorInfo defaultConstructor = controlType.GetConstructor(new Type[0]);
            UserControl result = (UserControl)defaultConstructor.Invoke(new object[0]);

            result.Width = double.NaN;
            result.Height = double.NaN;

            instances[controlType] = result;
            return result;
        }
    }
}
