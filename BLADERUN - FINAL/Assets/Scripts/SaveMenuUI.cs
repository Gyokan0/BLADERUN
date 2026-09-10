using UnityEngine;

public class SaveMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject savePanel;

    [Header("Dark Numbers")]
    [SerializeField] private GameObject save1Dark;
    [SerializeField] private GameObject save2Dark;
    [SerializeField] private GameObject save3Dark;

    [Header("Delete Cursor")]
    [SerializeField] private Texture2D deleteCursor;

    private bool deleteMode = false;

    private void Start()
    {
        UpdateSlots();
    }

    public void ToggleSavePanel()
    {
        savePanel.SetActive(!savePanel.activeSelf);

        deleteMode = false;

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        if (savePanel.activeSelf)
            UpdateSlots();
    }

    public void ToggleDeleteMode()
    {
        deleteMode = !deleteMode;

        if (deleteMode)
            Cursor.SetCursor(deleteCursor, Vector2.zero, CursorMode.Auto);
        else
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public void Save1()
    {
        SelectSlot(1);
    }

    public void Save2()
    {
        SelectSlot(2);
    }

    public void Save3()
    {
        SelectSlot(3);
    }

    private void SelectSlot(int slot)
    {
        if (deleteMode)
        {
            SaveSystem.DeleteSlot(slot);

            deleteMode = false;

            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

            UpdateSlots();
        }
        else
        {
            SaveSystem.SelectSlot(slot);
        }
    }

    private void UpdateSlots()
    {
        if (save1Dark != null)
            save1Dark.SetActive(SaveSystem.HasSave(1));

        if (save2Dark != null)
            save2Dark.SetActive(SaveSystem.HasSave(2));

        if (save3Dark != null)
            save3Dark.SetActive(SaveSystem.HasSave(3));
    }
}