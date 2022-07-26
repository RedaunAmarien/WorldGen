using UnityEngine.UIElements;
public class CharacterListEntryController
{
  private Label m_NameLabel;

  public void SetVisualElement(VisualElement visualElement)
  {
    m_NameLabel = visualElement.Q<Label>("CharacterName");
  }

  public void SetCharacterData(CharacterData characterData)
  {
    m_NameLabel.text = characterData.m_CharacterName;
  }
}