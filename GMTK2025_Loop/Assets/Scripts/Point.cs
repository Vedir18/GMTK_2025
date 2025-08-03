using UnityEngine;

public class Point : MonoBehaviour
{
    public int type;
    public bool Collected = false;

    private void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        type = Random.Range(1, 4);
        switch (type)
        {
            case 1:
                sr.color = Color.red;
                break;
            case 2:
                sr.color = Color.green;
                break;
            case 3:
                sr.color = Color.blue;
                break;
        }
        transform.rotation = Quaternion.AngleAxis(Random.Range(0f, 90f), Vector3.forward);
    }

    public void Collect()
    {
        Collected = true;
        gameObject.SetActive(false);
    }
}
