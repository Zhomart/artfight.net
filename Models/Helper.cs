﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

namespace ArtFight.Models
{
    public class Helper
    {
        private static Random random = new Random((int)DateTime.Now.Ticks);//thanks to McAden

        public static string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 'a')));
                builder.Append(ch);
            }

            return builder.ToString();
        }

    }
}