using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using Discord.Sdk;

public class FriendUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI friendNameText;

    [SerializeField]
    private TextMeshProUGUI friendStatusText;

    [SerializeField]
    private Image friendAvatarImage;

    private Client client;
    public RelationshipHandle relationshipHandle { get; private set; }

    public void Initialize(Client client, RelationshipHandle relationshipHandle)
    {
        this.client = client;
        this.relationshipHandle = relationshipHandle;
        friendNameText.text = relationshipHandle.User().DisplayName();
        friendStatusText.text = relationshipHandle.User().Status().ToString();
        StartCoroutine(LoadAvatarFromUrl(relationshipHandle.User().AvatarUrl(UserHandle.AvatarType.Png, UserHandle.AvatarType.Png)));
    }

    public void UpdateFriend()
    {
        friendNameText.text = relationshipHandle.User().DisplayName();
        friendStatusText.text = relationshipHandle.User().Status().ToString();
    }

    private IEnumerator LoadAvatarFromUrl(string url)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                friendAvatarImage.sprite = sprite;
            }
            else
            {
                Debug.LogError($"Failed to load profile image from URL: {url}. Error: {request.error}");
            }
        }
    }
}
