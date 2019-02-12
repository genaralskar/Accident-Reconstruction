using UnityEngine;

[ExecuteInEditMode]
public class PositionHandleExample : MonoBehaviour
{
    public Vector3 targetPosition { get { return m_TargetPosition; } set { m_TargetPosition = value; } }
    [SerializeField]
    private Vector3 m_TargetPosition = new Vector3(1f, 0f, 2f);

    private void OnEnable()
    {
        targetPosition = transform.position;
    }

    public virtual void Update()
    {
//        transform.LookAt(m_TargetPosition);
        transform.position = m_TargetPosition;
    }
}