using System;

namespace _Scripts
{
    public static class Converter 
    {
        public static int[] ToIntArray(byte[] inputElements)
        {
            int[] myFinalIntegerArray = new int[inputElements.Length / 4];
            for (int cnt = 0; cnt < inputElements.Length; cnt += 4)
            {
                myFinalIntegerArray[cnt / 4] = BitConverter.ToInt32(inputElements, cnt);
            }
            return myFinalIntegerArray;
        }
    
        public static byte[] ToByteArray(int[] inputElements)
        {
            byte[] myFinalBytes = new byte[inputElements.Length * 4];
            for(int cnt = 0 ; cnt<inputElements.Length; cnt ++)
            {
                byte[] myBytes = BitConverter.GetBytes(inputElements[cnt]);
                Array.Copy(myBytes, 0, myFinalBytes, cnt*4, 4);
            }
            return myFinalBytes;
        }
    }
}
