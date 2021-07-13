namespace CrypTool.Chaocipher.Enums
{
    public enum Step
    {
        Begin = 0,
        BringCharToZenith = 1,
        BringCipherCharToZenith = 2,
        CharBroughtToZenith = 10,
        CipherCharBroughtToZenith = 11,
        CharCanNotBeEnciphered = 12,
        CharCanNotBeDeciphered = 13,
        PermutateLeftDisk = 100,
        PermutateLeftDiskRemoveChar = 110,
        PermutateLeftDiskMoveChars = 120,
        PermutateLeftDiskInsertChar = 130,
        PermutateRightDisk = 200,
        PermutateRightDiskRemoveChar = 210,
        PermutateRightDiskMoveByOne = 211,
        PermutateRightDiskMoveChars = 220,
        PermutateRightDiskInsertChar = 230,
        NotPermutateOnLastChar = 300,
        End = 10000,
    }
}