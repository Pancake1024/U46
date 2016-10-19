using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Math;

namespace SBS.Racing
{
#if UNITY_FLASH
    public class TrackBranch : IComparable
#else
	public class TrackBranch : IComparable<TrackBranch>
#endif
	{
        #region BranchToken class
#if UNITY_FLASH
        protected class BranchToken : IComparable
#else
        protected class BranchToken : IComparable<BranchToken>
#endif
        {
            public Token token;
            public float startLength;

            private BranchToken()
            {
            }

            public BranchToken(Token _token, float _startLength)
            {
                token = _token;
                startLength = _startLength;
            }

#if UNITY_FLASH
            public int CompareTo(object other)
            {
			    float otherStartLen = (other as BranchToken).startLength;
			    if (startLength < otherStartLen)
				    return -1;
			    else if (startLength > otherStartLen)
				    return 1;
			    else
				    return 0;
            }
#else
            public int CompareTo(BranchToken other)
            {
                return startLength.CompareTo((other as BranchToken).startLength);
            }
#endif
        };
        #endregion

        #region Public members
        public float startLength = 0.0f;
        public int mask = -1;

        protected float lenScale = 1.0f;
        protected float invLenScale = 1.0f;
        #endregion

        #region Protected members
        protected float length = 0.0f;
        protected List<BranchToken> branchTokens = new List<BranchToken>();
        #endregion

        #region Public properties
        public float Length
        {
            get
            {
                return length;
            }
        }

        public float LengthScale
        {
            get
            {
                return lenScale;
            }
            set
            {
                lenScale = value;
                invLenScale = 1.0f / lenScale;
            }
        }

        public int Count
        {
            get
            {
                return branchTokens.Count;
            }
        }

        public Token this[int tokenIndex]
        {
            get
            {
                return branchTokens[tokenIndex].token;
            }
        }
        #endregion

        #region Public methods
        public void Clear()
        {
            length = 0.0f;
            branchTokens.Clear();
        }

        public void AddToken(Token token)
        {
            token.LinkToTrack(this, branchTokens.Count);
            branchTokens.Add(new BranchToken(token, length));
            length += token.Length;
        }

#if UNITY_FLASH
        public Token EvaluateAt(float offset, float trasversal, SBSVector3 pos, SBSVector3 tang)
#else
        public Token EvaluateAt(float offset, float trasversal, out SBSVector3 pos, out SBSVector3 tang)
#endif
        {
            BranchToken dummy = new BranchToken(null, (offset - startLength) * invLenScale);

#if UNITY_FLASH
            int index = AS3Utils.BinarySearchList(branchTokens, dummy);
#else
            int index = branchTokens.BinarySearch(dummy);
#endif
            index = (index < 0 ? ~index : index);

            BranchToken branchToken = branchTokens[Mathf.Clamp(index - 1, 0, branchTokens.Count - 1)];
            float l = (dummy.startLength - branchToken.startLength) / branchToken.token.Length;
		    l = Mathf.Clamp01(l);
#if UNITY_FLASH
            branchToken.token.TokenToWorld(l, trasversal, pos, tang);
#else
            branchToken.token.TokenToWorld(l, trasversal, out pos, out tang);
#endif
            return branchToken.token;
        }

        public float GetTrackPosition(Token token, float longitudinal)
        {
            Asserts.Assert(this == token.TrackBranch);
            BranchToken branchToken = branchTokens[token.TrackBranchIndex];
            return startLength + (branchToken.startLength + token.Length * longitudinal) * lenScale;
        }

#if UNITY_FLASH
        public int CompareTo(object other)
        {
            float otherStartLen = (other as TrackBranch).startLength;
            if (startLength < otherStartLen)
                return -1;
            else if (startLength > otherStartLen)
                return 1;
            else
                return 0;
        }
#else
        public int CompareTo(TrackBranch other)
        {
            return startLength.CompareTo(other.startLength);
        }
#endif
        #endregion
    }
}
