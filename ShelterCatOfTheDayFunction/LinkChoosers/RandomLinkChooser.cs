using System;

namespace ShelterCatOfTheDayFunction.LinkChoosers
{
    public class RandomLinkChooser : ILinkChooser
    {
        private readonly Random _randomGenerator = new Random();

        public string Choose(string[] links)
        {
            var randomIndex = _randomGenerator.Next(0, links.Length);

            return links[randomIndex];
        }
    }
}