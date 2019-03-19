using System;

public partial class PunchCombo
{
    private struct ComboPlaceMarker
    {
        private PunchStats[] combo;
        private int currentIndex;
        public bool IsAtEnd
        {
            get
            {
                return combo == null || currentIndex >= combo.Length;
            }
        }
        public bool IsEmpty
        {
            get
            {
                return combo == null || combo.Length == 0;
            }
        }

        public void SetCombo( PunchStats[] combo)
        {
            if(this.combo != combo)
            {
                this.combo = combo;
                currentIndex = 0;
            }
        }

        public PunchStats GetCurrent()
        {
            if (IsAtEnd)
                throw new IndexOutOfRangeException("Trying to getCurrent in a combo that is at its end! Check for this condition using the IsAtEnd method");

            return (combo[currentIndex]);
        }

        /// <summary>
        /// returns current and advances index to next slot
        /// </summary>
        /// <returns></returns>
        public bool AdvanceCombo()
        {
            var result = IsAtEnd;
            if(!IsAtEnd) currentIndex++;
            return result;
        }


        public void Reset()
        {
            currentIndex = 0;
        }
    }
}








