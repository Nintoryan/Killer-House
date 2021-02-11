using UnityEngine;

namespace UserData
{
    public static class Wallet
    {
        public static int Balance
        {
            get => PlayerPrefs.GetInt("Money");
            set => PlayerPrefs.SetInt("Money", value);
        }
    }

    public static class Statistics
    {
        public static int Wins
        {
            get => PlayerPrefs.GetInt("Wins");
            set => PlayerPrefs.SetInt("Wins",value);
        }
    }
}

