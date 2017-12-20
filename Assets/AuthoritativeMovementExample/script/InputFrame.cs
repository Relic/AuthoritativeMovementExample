using System;

namespace AuthMovementExample
{
    /*
     * A serializable frame containing the polled input state
     * 
     * Stored for prediction/reconciliation and sent to the server for processing
     */
    [Serializable]
    public class InputFrame
    {
        public uint frameNumber;
        public bool right;
        public bool down;
        public bool left;
        public bool up;
        public float horizontal;
        public float vertical;

        public bool HasInput { get { return right || down || left || up; } }

        // Equality operator overload - we want to check if input has changed
        // and we don't care about the frameNumber, timestamp or input axis
        public static bool operator ==(InputFrame i1, InputFrame i2)
        {
            // Null & reference check (i1 and i2 are the same if they're at the same location in memory obviously)
            if (ReferenceEquals(i1, i2)) return true;
            else if (ReferenceEquals(i1, null) || ReferenceEquals(i2, null)) return false;

            // Check the inputs
            return i1.right == i2.right &&
                i1.down == i2.down &&
                i1.left == i2.left &&
                i1.up == i2.up && 
                i1.horizontal == i2.horizontal &&
                i1.vertical == i2.vertical;
        }

        // Inequality operator overload - see above, but inverted
        public static bool operator !=(InputFrame i1, InputFrame i2)
        {
            // Null & reference check (i1 and i2 are the same if they're at the same location in memory obviously)
            if (ReferenceEquals(i1, i2)) return false;
            else if (ReferenceEquals(i1, null) || ReferenceEquals(i2, null)) return true;

            // Check the inputs
            return i1.right != i2.right ||
                i1.down != i2.down ||
                i1.left != i2.left ||
                i1.up != i2.up ||
                i1.horizontal != i2.horizontal ||
                i1.vertical != i2.vertical;
        }

        // Equals() should be identical to the equality operator
        public override bool Equals(object obj)
        {
            // Null & reference check (i1 and i2 are the same if they're at the same location in memory obviously)
            if (ReferenceEquals(this, obj)) return true;
            else if (ReferenceEquals(this, null) || ReferenceEquals(obj, null)) return false;

            // Check the inputs
            return right == ((InputFrame)obj).right &&
                down == ((InputFrame)obj).down &&
                left == ((InputFrame)obj).left &&
                up == ((InputFrame)obj).up && 
                horizontal == ((InputFrame)obj).horizontal &&
                vertical == ((InputFrame)obj).vertical;
        }

        // GetHashCode() should correspond to Equals() and the equality operator
        public override int GetHashCode()
        {
            return right.GetHashCode() ^ down.GetHashCode() ^ left.GetHashCode() ^
                up.GetHashCode() ^ horizontal.GetHashCode() ^ vertical.GetHashCode();
        }
    }
}