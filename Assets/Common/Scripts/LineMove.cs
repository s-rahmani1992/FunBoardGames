using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace OnlineBoardGames
{
    public enum MoveMode
    {
        FixedTime,
        Scaled
    }

    public class LineMove : MonoBehaviour
    {
        public void MoveInLine(Vector2 target, MoveMode mode, float speed, System.Action<LineMove> EndReached = null, float delay = 0){
            if (mode == MoveMode.FixedTime)
                StartCoroutine(MoveBetweenPoint(transform.localPosition, target, 1.0f / speed, EndReached, delay));
            else if (mode == MoveMode.Scaled)
                StartCoroutine(MoveBetweenPoint(transform.localPosition, target, speed / Vector2.Distance((Vector2)transform.localPosition, target), EndReached, delay));
        }

        IEnumerator MoveBetweenPoint(Vector2 start, Vector2 end, float speed, System.Action<LineMove> EndReached = null, float delay = 0){
            Vector2 current = start;
            float t = 0;
            yield return new WaitForSeconds(delay);
            if (Vector2.Distance(start, end) > 0.05f){
                while (t <= 1){
                    current = (1 - t) * start + t * end;
                    transform.localPosition = new Vector3(current.x, current.y, transform.localPosition.z);
                    t += speed * Time.deltaTime;
                    yield return null;
                }
            }

            transform.localPosition = new Vector3(end.x, end.y, transform.localPosition.z);
            EndReached?.Invoke(this);
        }
    }
}
