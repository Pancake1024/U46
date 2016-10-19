using UnityEngine;

[RequireComponent(typeof(Token))]
public class TokenBakedLighting : MonoBehaviour
{
    public int gridLongitudinalSize = 10;
    public TokenBakedLightingSamples samples = null;

    protected int gridTrasversalSize;

    void Awake()
    {
        Token token = gameObject.GetComponent<Token>();
        gridTrasversalSize = Mathf.RoundToInt(gridLongitudinalSize * token.width / token.Length);
    }

    public void TakeSamples()
    {
        this.Awake();

        samples = new TokenBakedLightingSamples();
        samples.data = new byte[gridLongitudinalSize * gridTrasversalSize * 3];

        float longStep = 1.0f / (float)gridLongitudinalSize,
              trasStep = 1.0f / (float)gridTrasversalSize;
        float tl = longStep * 0.5f;
        for (int l = 0; l < gridLongitudinalSize; ++l)
        {
            float tt = trasStep * 0.5f;
            for (int t = 0; t < gridTrasversalSize; ++t)
            {
                tt += trasStep;
            }
            tl += longStep;
        }
    }
}
