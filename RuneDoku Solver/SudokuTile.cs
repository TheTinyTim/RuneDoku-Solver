﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuneDoku_Solver
{
    public class SudokuTile
    {
        internal static RuneDoku_Solver.Form1.SudokuProgress CombineSolvedState(RuneDoku_Solver.Form1.SudokuProgress a, RuneDoku_Solver.Form1.SudokuProgress b)
        {
            if (a == RuneDoku_Solver.Form1.SudokuProgress.FAILED)
                return a;
            if (a == RuneDoku_Solver.Form1.SudokuProgress.NO_PROGRESS)
                return b;
            if (a == RuneDoku_Solver.Form1.SudokuProgress.PROGRESS)
                return b == RuneDoku_Solver.Form1.SudokuProgress.FAILED ? b : a;
            throw new InvalidOperationException("Invalid value for a");
        }

        public const int CLEARED = 0;
        private int _maxValue;
        private int _value;
        private int _x;
        private int _y;
        private ISet<int> possibleValues;
        private bool _blocked;

        public SudokuTile(int x, int y, int maxValue)
        {
            _x = x;
            _y = y;
            _blocked = false;
            _maxValue = maxValue;
            possibleValues = new HashSet<int>();
            _value = 0;
        }

        public int Value
        {
            get { return _value; }
            set
            {
                if (value > _maxValue)
                    throw new ArgumentOutOfRangeException("SudokuTile Value cannot be greater than " + _maxValue.ToString() + ". Was " + value);
                if (value < CLEARED)
                    throw new ArgumentOutOfRangeException("SudokuTile Value cannot be zero or smaller. Was " + value);
                _value = value;
            }
        }

        public bool HasValue
        {
            get { return Value != CLEARED; }
        }

        public string ToStringSimple()
        {
            return Value.ToString();
        }

        public override string ToString()
        {
            return String.Format("Value {0} at pos {1}, {2}. ", Value, _x, _y, possibleValues.Count);
        }

        internal void ResetPossibles()
        {
            possibleValues.Clear();
            foreach (int i in Enumerable.Range(1, _maxValue))
            {
                if (!HasValue || Value == i)
                    possibleValues.Add(i);
            }
        }

        public void Block()
        {
            _blocked = true;
        }
        internal void Fix(int value, string reason)
        {
            Console.WriteLine("Fixing {0} on pos {1}, {2}: {3}", value, _x, _y, reason);
            Value = value;
            ResetPossibles();
        }
        internal RuneDoku_Solver.Form1.SudokuProgress RemovePossibles(IEnumerable<int> existingNumbers)
        {
            if (_blocked)
                return RuneDoku_Solver.Form1.SudokuProgress.NO_PROGRESS;
            // Takes the current possible values and removes the ones existing in `existingNumbers`
            possibleValues = new HashSet<int>(possibleValues.Where(x => !existingNumbers.Contains(x)));
            RuneDoku_Solver.Form1.SudokuProgress result = RuneDoku_Solver.Form1.SudokuProgress.NO_PROGRESS;
            if (possibleValues.Count == 1)
            {
                Fix(possibleValues.First(), "Only one possibility");
                result = RuneDoku_Solver.Form1.SudokuProgress.PROGRESS;
            }
            if (possibleValues.Count == 0)
                return RuneDoku_Solver.Form1.SudokuProgress.FAILED;
            return result;
        }

        public bool IsValuePossible(int i)
        {
            return possibleValues.Contains(i);
        }

        public int X { get { return _x; } }
        public int Y { get { return _y; } }
        public bool IsBlocked { get { return _blocked; } } // A blocked field can not contain a value -- used for creating 'holes' in the map
        public int PossibleCount
        {
            get
            {
                return IsBlocked ? 1 : possibleValues.Count;
            }
        }
    }
}
