using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Math;

namespace SBS.Level
{
    public class LevelCell
    {
        #region Private members
        private SBSBounds bounds;
        private List<LevelObject> objects;
        private Dictionary<string, int> numObjectsByCat;
        private int[] numObjectsByMask;
        private LevelCell parent;
        private List<LevelCell> children;
        #endregion

        #region Ctors
        private LevelCell()
        {
        }

        public LevelCell(SBSBounds _bounds)
        {
            bounds = _bounds;
            objects = new List<LevelObject>();
            numObjectsByCat = new Dictionary<string, int>();
            numObjectsByMask = new int[32];
            parent = null;
            children = new List<LevelCell>();

            for (int bit = 0; bit < 32; ++bit)
                numObjectsByMask[bit] = 0;
        }
        #endregion

        #region Public properties
        public SBSBounds Bounds
        {
            get
            {
                return bounds;
            }
        }

        public IEnumerable<LevelObject> Objects
        {
            get
            {
                return objects;
            }
        }

        public LevelCell Parent
        {
            get
            {
                return parent;
            }
            set
            {
                if (parent != null)
                    parent.children.Remove(this);

                parent = value;

                if (parent != null)
                    parent.children.Add(this);
            }
        }

        public IEnumerable<LevelCell> Children
        {
            get
            {
                return children;
            }
        }
        #endregion

        #region Protected methods
        protected void UpdateNumObjectsOfCategory(string category, int delta)
        {
            if (null == category)
                return;

            if (!numObjectsByCat.ContainsKey(category))
                numObjectsByCat.Add(category, 0);

            numObjectsByCat[category] += delta;

            if (parent != null)
                parent.UpdateNumObjectsOfCategory(category, delta);
        }

        protected void UpdateNumObjectsWithMask(int mask, int delta)
        {
            if (0 == mask)
                return;

            int bit = 1;
            for (int counter = 0; counter < 32; ++counter)
            {
                if ((mask & bit) != 0)
                    numObjectsByMask[counter] += delta;

                bit <<= 1;
            }

            if (parent != null)
                parent.UpdateNumObjectsWithMask(mask, delta);
        }
        #endregion

        #region Public methods
        public int GetNumObjectsOfCategory(string category)
        {
            int count;
            if (numObjectsByCat.TryGetValue(category, out count))
                return count;
            else
                return 0;
        }

        public int GetNumObjectsWithMask(int mask)
        {
            int count = 0;
            int bit = 1;
            for (int counter = 0; counter < 32; ++counter)
            {
                if ((mask & bit) != 0)
                    count += numObjectsByMask[counter];

                bit <<= 1;
            }
            return count;
        }
        #endregion

        #region LevelObject functions
        public void AttachObject(LevelObject obj)
        {
            objects.Add(obj);

            this.UpdateNumObjectsOfCategory(obj.Category, +1);
            this.UpdateNumObjectsWithMask(obj.LayersMask, +1);
        }

        public void DetachObject(LevelObject obj)
        {
            objects.Remove(obj);

            this.UpdateNumObjectsOfCategory(obj.Category, -1);
            this.UpdateNumObjectsWithMask(obj.LayersMask, -1);
        }

        public void NotifyObjectCategory(LevelObject obj, string oldCat, string newCat)
        {
            this.UpdateNumObjectsOfCategory(oldCat, -1);
            this.UpdateNumObjectsOfCategory(newCat, +1);
        }

        public void NotifyObjectLayersMask(LevelObject obj, int oldMask, int newMask)
        {
            this.UpdateNumObjectsWithMask(oldMask, -1);
            this.UpdateNumObjectsWithMask(newMask, +1);
        }

        public LevelCell FindObjectContainmentCell(LevelObject obj)
        {
            SBSBounds objBounds = obj.Bounds;

            LevelCell curCell = this;
            while (curCell.parent != null && !curCell.bounds.Contains(objBounds))
            {
                curCell = curCell.parent;
            }

            int cellIndex, numCells;
            do
            {
                List<LevelCell> curChildren = curCell.children;
                numCells = curChildren.Count;
                for (cellIndex = 0; cellIndex < numCells; ++cellIndex)
                {
                    LevelCell child = curChildren[cellIndex];
                    if (child.bounds.Contains(objBounds))
                    {
                        curCell = child;
                        break;
                    }
                }
            }
            while (cellIndex != numCells);

            return curCell;
        }
        #endregion

        #region Query functions
        public void RecurseQuery(SBSBounds bbox, SBSMath.ClipStatus clipStatus, string category, int mask, List<LevelObject> results)
        {
            if (!(null == category) && 0 == this.GetNumObjectsOfCategory(category))
                return;

            if (!(-1 == mask) && 0 == this.GetNumObjectsWithMask(mask))
                return;

            if (SBSMath.ClipStatus.Overlapping == clipStatus)
            {
                clipStatus = SBSMath.GetClipStatus(bbox, bounds);
            }

            if (SBSMath.ClipStatus.Outside == clipStatus)
            {
                return;
            }
            else
            {
                bool doMaskCheck = !(-1 == mask);
                bool doCategoryCheck = !(null == category);
                bool doClipCheck = (SBSMath.ClipStatus.Overlapping == clipStatus);

                foreach (LevelObject obj in objects)
                {
                    if (doMaskCheck && 0 == (mask & obj.LayersMask))
                        continue;

                    if (doCategoryCheck && category != obj.Category)
                        continue;

                    if (doClipCheck && SBSMath.ClipStatus.Outside == SBSMath.GetClipStatus(bbox, obj.Bounds))
                        continue;

                    results.Add(obj);
                }
            }

            foreach (LevelCell child in children)
            {
                child.RecurseQuery(bbox, clipStatus, category, mask, results);
            }
        }
        #endregion
    }
}
