using UnityEngine;
using SBS.Core;

[AddComponentMenu("OnTheRun/UI/UIPageMarks")]
public class UIPageMarks : MonoBehaviour
{
    public GameObject markPrefab;

    float markWidth = 26.0f;

    UIMark[] marks;

    void CreateMarks(int marksNumber)
    {
        //mark width = 26;
        float offsetX = markWidth;
        float totalXoffset = offsetX * (marksNumber - 1);
        float firstX = marksNumber * markWidth * 0.5f + totalXoffset * 0.5f;
        marks = new UIMark[marksNumber];
        for (int i = 0; i < marksNumber; i++)
        {
            GameObject currentMark;
            currentMark = GameObject.Instantiate(markPrefab) as GameObject;
            currentMark.transform.parent = this.transform;
            currentMark.transform.position = new Vector3(firstX + i*offsetX, 0.0f, 0.0f);
            marks[i] = currentMark.GetComponent<UIMark>();
            marks[i].SetMarkActive(false);
        }
    }

    public void ShowMarks(int pagesNumber, int currentPage)
    {
        if (pagesNumber <= 0 || currentPage < 0)
        {
            Debug.LogWarning(pagesNumber <= 0 ? "Impossible to set marks with number of page = 0" : "WARNING: currentPage can't be < 0");
            return;
        }

        CreateMarks(pagesNumber);
        SetPageMark(currentPage);
    }

    public void SetPageMark(int page)
    {
        for (int i = 0; i < marks.Length; i++)
        {
            marks[i].SetMarkActive(false);
        }
        marks[page].SetMarkActive(true);
    }
}