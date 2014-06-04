using System;
namespace ConsoleApplication2
{

    class Sorting
    {
        public void BubbleSort(int[] num)
        {
            bool flag = true;
            int i = 0;
            int temp;
            while (flag)
            {
                flag = false;
                for (i = 0; i < num.Length - 1; i++)
                {
                    if (num[i] > num[i + 1])
                    {
                        //swap
                        temp = num[i];
                        num[i] = num[i + 1];
                        num[i + 1] = temp;
                        flag = true;
                    }
                }
            }

        }
    }
}