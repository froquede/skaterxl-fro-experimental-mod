using GameManagement;
using HarmonyLib;
using ModIO.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityModManagerNet;

namespace fro_mod
{
    public class ChatBubbleTest : MonoBehaviour
    {
        MultiplayerChatManager CM;

        public float up, down, left, right;
        bool open = true;
        int index = 0;
        int limit = 48;
        float lastChatOpenTime;
        float last_sent_message = 0;

        public void Start()
        {
            CM = MultiplayerManager.Instance.chatManager;
            CM.messageDuration = 6f;
            gameObject.name = "ChatBubbleTest";
            UnityModManager.Logger.Log("CBT initialized: " + gameObject.name + " " + CM.isActiveAndEnabled);
            MultiplayerInteractionDatabase.Instance.UpdateChatMessages();
            lastChatOpenTime = Time.unscaledTime;

            for (int i = 0; i < MultiplayerInteractionDatabase.Instance.availableChatMessages.Length; i++)
            {
                MultiplayerInteractionDatabase.ChatMessageTemplate message = MultiplayerInteractionDatabase.Instance.availableChatMessages[i];
                //UnityModManager.Logger.Log(message.ToString() + " " + message.message + " " + message.animationName + " " + message.emoteGifAnim + " " + message.emoteImage);
            }

        }

        float last_message = 0f;
        public bool checkLastMessage()
        {
            if (Time.unscaledTime - last_message >= 20f)
            {
                last_message = Time.unscaledTime;
                return true;
            }
            else
            {
                MessageSystem.QueueMessage(MessageDisplayData.Type.Warning, "You can only send messages every 20 seconds, last message sent " + (Time.unscaledTime - last_message).ToString("N0") + " seconds ago", 1.5f);
                return false;
            }
        }


        public void Update()
        {
            if (GameStateMachine.Instance.CurrentState.GetType() == typeof(ReplayState) || !Main.settings.chat_messages) return;
            if (MonoBehaviourSingleton<PopupManager>.Instance.IsOpen || MonoBehaviourSingleton<PromotionController>.Instance.IsOpen || MonoBehaviourSingleton<ConsentManager>.Instance.IsOpen) return;

            if (Time.unscaledTime - last_sent_message <= 20f)
            {
                index = 0;
                open = true;
                return;
            }

            if (PlayerController.Instance.inputController.player.GetButton(69)) right += 1;
            else right = 0;

            if (PlayerController.Instance.inputController.player.GetButton(70)) left += 1;
            else left = 0;

            if (PlayerController.Instance.inputController.player.GetButton(67)) up += 1;
            else up = 0;

            if (PlayerController.Instance.inputController.player.GetButton(68)) down += 1;
            else down = 0;

            if (open)
            {
                if (right > limit || left > limit || up > limit || down > limit) DefineOpen(left, right, up, down);
            }
            else
            {
                if ((right > 1 || left > 1 || up > 1 || down > 1))
                {
                    last_sent_message = Time.unscaledTime;
                    index = 0;
                    open = true;
                }
                if (PlayerController.Instance.inputController.player.GetButton("B"))
                {
                    index = 0;
                    open = true;
                }
            }

            CMUpdate();
        }

        public void FixedUpdate()
        {
            UpdateSpeechBubbles();
        }

        void DefineOpen(float left, float right, float up, float down)
        {
            if (left > 0) index = Main.settings.left_page;
            if (right > 0) index = Main.settings.right_page;
            if (up > 0) index = Main.settings.up_page;
            if (down > 0) index = Main.settings.down_page;
            open = false;
        }

        void CMUpdate()
        {
            bool flag = true;
            bool flag2 = false;

            int pad_index = index;

            if (pad_index > 5) pad_index = pad_index + 1;

            if (!open) OpenChatWheel(pad_index);

            if (CM.chatListCanvas.activeSelf != flag2)
            {
                if (flag2)
                {
                    CM.listView.scrollRect.verticalNormalizedPosition = 0f;
                }
                CM.chatListCanvas.SetActive(flag2);
            }
        }

        List<ChatSpeechBubble> chatBubbles = new List<ChatSpeechBubble>();
        private void UpdateSpeechBubbles()
        {
            chatBubbles = (List<ChatSpeechBubble>)Traverse.Create(CM).Field("chatBubbles").GetValue();
            CM.fadeOutSpeed = .75f;
            // UnityModManager.Logger.Log("Updating speech bubbles " + chatBubbles.Count);

            for (int i = 0; i < chatBubbles.Count; i++)
            {
                ChatSpeechBubble chatSpeechBubble = chatBubbles[i];
                chatSpeechBubble.BubbleScale = .6f;
                chatSpeechBubble.arrowTipSpace = 8f;
                chatSpeechBubble.bubbleToTargetSpace = 8f;
                chatSpeechBubble.spectatorChatScale = .6f;
                chatSpeechBubble.messageDisplay.aspectRatio = 2f;
                chatSpeechBubble.minDist = 0.1f;
                chatSpeechBubble.maxDist = 20f;
                chatSpeechBubble.canvasMargin = new Vector2(1f, 1f);

                bool visible = chatSpeechBubble.Visible;
                chatSpeechBubble.UpdateTarget(Camera.main);
                bool flag = Time.unscaledTime > chatSpeechBubble.endTime;
                if ((!chatSpeechBubble.validTarget && !chatSpeechBubble.targetingLocal) || flag)
                {
                    chatSpeechBubble.Fadeout(true, CM.fadeOutSpeed);
                    if (!chatSpeechBubble.Visible)
                    {
                        if (flag)
                        {
                            chatBubbles.RemoveAt(i);
                            i--;
                        }
                        else
                        {
                            chatSpeechBubble.canvas.enabled = false;
                        }
                    }
                }
                else
                {
                    chatSpeechBubble.canvas.enabled = true;
                    if (chatSpeechBubble.validTarget)
                    {
                        UpdateArrowTarget(Camera.main, chatSpeechBubble);
                    }
                    chatSpeechBubble.Fadeout(false, CM.fadeOutSpeed);
                    if (!visible)
                    {
                        chatSpeechBubble.bubblePosition = chatSpeechBubble.arrowTarget + Vector3.up * chatSpeechBubble.BubbleSize.y * 0.6f;
                    }
                }
            }
            for (int j = 0; j < chatBubbles.Count; j++)
            {
                ChatSpeechBubble chatSpeechBubble2 = chatBubbles[j];
                AddArrowBubbleForce(chatSpeechBubble2);
                if (chatSpeechBubble2.Visible)
                {
                    for (int k = j + 1; k < chatBubbles.Count; k++)
                    {
                        ChatSpeechBubble chatSpeechBubble3 = chatBubbles[k];
                        if (chatSpeechBubble3.Visible)
                        {
                            AddBubbleBubbleForce(chatSpeechBubble2, chatSpeechBubble3);
                        }
                    }
                }
            }
            float num = 0f;
            foreach (ChatSpeechBubble chatSpeechBubble4 in chatBubbles)
            {
                if (chatSpeechBubble4.fixedInTopRight || (chatSpeechBubble4.currentMessage.author != null && chatSpeechBubble4.currentMessage.author.SpectatedPlayer != null && (chatSpeechBubble4.currentMessage.author.SpectatedPlayer.IsLocal || chatSpeechBubble4.currentMessage.author.SpectatedPlayer == MonoBehaviourPunCallbacksSingleton<MultiplayerManager>.Instance.localPlayer.SpectatedPlayer)) || (!chatSpeechBubble4.validTarget && chatSpeechBubble4.targetingLocal))
                {
                    chatSpeechBubble4.bubblePosition = Vector3.Lerp(chatSpeechBubble4.bubblePosition, chatSpeechBubble4.CanvasSize - chatSpeechBubble4.BubbleSize / 2f - CM.canvasMargin - new Vector2(0f, num), 10f * Time.unscaledDeltaTime);
                    num += chatSpeechBubble4.BubbleSize.y + 8f;
                    chatSpeechBubble4.MoveUiElements(false);
                }
                else
                {
                    chatSpeechBubble4.bubblePosition += chatSpeechBubble4.bubbleVelocity * Time.unscaledDeltaTime;
                    chatSpeechBubble4.bubblePosition.x = Mathf.Clamp(chatSpeechBubble4.bubblePosition.x, CM.canvasMargin.x + chatSpeechBubble4.BubbleSize.x / 2f, chatSpeechBubble4.CanvasSize.x - (CM.canvasMargin.x + chatSpeechBubble4.BubbleSize.x / 2f));
                    chatSpeechBubble4.bubblePosition.y = Mathf.Clamp(chatSpeechBubble4.bubblePosition.y, CM.canvasMargin.y + chatSpeechBubble4.BubbleSize.y / 2f, chatSpeechBubble4.CanvasSize.y - (CM.canvasMargin.y + chatSpeechBubble4.BubbleSize.y / 2f));
                    chatSpeechBubble4.bubbleVelocity *= Mathf.Clamp01(1f - CM.damping * Time.unscaledDeltaTime);
                    chatSpeechBubble4.MoveUiElements(true);
                }

                chatSpeechBubble4.bubblePosition.y += 2f;

                //chatSpeechBubble4.bubblePosition.y = PlayerController.Instance.gameObject.transform.position.y - 10f;
            }
            int num2 = 0;
            foreach (ChatSpeechBubble chatSpeechBubble5 in from b in chatBubbles
                                                           where b.Visible
                                                           orderby b.viewPortZ - (Time.unscaledTime - b.endTime) * CM.timeSortingFactor
                                                           select b)
            {
                chatSpeechBubble5.canvas.sortingOrder = num2;
                num2++;
            }
        }
        public void UpdateArrowTarget(Camera camera, ChatSpeechBubble bubble)
        {
            bubble.arrowTarget = camera.WorldToViewportPoint(bubble.arrowTargetTransform.position);
            Traverse.Create(bubble).Field("viewPortZ").SetValue(bubble.arrowTarget.z);
            float viewPortZ = bubble.viewPortZ;
            if (bubble.arrowTarget.z < 0f)
            {
                bubble.arrowTarget *= -1f;
                bubble.arrowTarget.x = bubble.arrowTarget.x + 1f;
                bubble.arrowTarget.y = bubble.arrowTarget.y + 2f;
            }
            bubble.arrowTarget.x = bubble.arrowTarget.x * bubble.CanvasSize.x;
            bubble.arrowTarget.y = bubble.arrowTarget.y * bubble.CanvasSize.y;
            bubble.arrowTarget.z = 0f;
        }

        void AddBubbleBubbleForce(ChatSpeechBubble a, ChatSpeechBubble b)
        {
            Vector3 vector = a.bubblePosition - b.bubblePosition;
            Vector2 vector2 = a.BubbleSize + b.BubbleSize;
            if (Mathf.Abs(vector.x) < 0.6f * vector2.x && Mathf.Abs(vector.y) < 0.6f * vector2.y)
            {
                if (Mathf.Abs(vector.x) / vector2.x > Mathf.Abs(vector.y) / vector2.y)
                {
                    float time = Mathf.Abs(vector.x) / (0.5f * vector2.x);
                    float d = CM.bubbleBubbleForce.Evaluate(time);
                    float num = Mathf.Sign(vector.x);
                    if (Mathf.Abs(num) < 0.5f)
                    {
                        num = 1f;
                    }
                    a.bubbleVelocity += new Vector3(num, 0f) * d * CM.bbForceFactor * Time.unscaledDeltaTime;
                    b.bubbleVelocity += new Vector3(-num, 0f) * d * CM.bbForceFactor * Time.unscaledDeltaTime;
                    return;
                }
                float time2 = Mathf.Abs(vector.y) / (0.5f * vector2.y);
                float d2 = CM.bubbleBubbleForce.Evaluate(time2);
                float num2 = Mathf.Sign(vector.y);
                if (Mathf.Abs(num2) < 0.5f)
                {
                    num2 = 1f;
                }
                a.bubbleVelocity += new Vector3(0f, num2) * d2 * CM.bbForceFactor * Time.unscaledDeltaTime;
                b.bubbleVelocity += new Vector3(0f, -num2) * d2 * CM.bbForceFactor * Time.unscaledDeltaTime;
            }
        }

        void AddArrowBubbleForce(ChatSpeechBubble bubble)
        {
            Vector3 vector = bubble.bubblePosition - bubble.arrowTarget;
            float time = 1f * vector.magnitude / bubble.BubbleSize.magnitude;
            float d = CM.arrowBubbleForce.Evaluate(time);
            bubble.bubbleVelocity += vector.normalized * d * CM.abForceFactor * Time.unscaledDeltaTime;
        }

        private void OpenChatWheel(int i)
        {
            Traverse.Create(CM).Field("currentSet").SetValue(i);
            CM.dpadController.SetTexts(GetMessage(i * 4), GetMessage(i * 4 + 1), GetMessage(i * 4 + 2), GetMessage(i * 4 + 3));
            CM.dpadController.gameObject.SetActive(true);
            Traverse.Create(CM).Field("lastChatOpenTime").SetValue(Time.unscaledTime);
            open = true;
            lastChatOpenTime = Time.unscaledTime;
        }

        void CloseChatWheel()
        {
            CM.dpadController.gameObject.SetActive(false);
            Traverse.Create(CM).Field("currentSet").SetValue(-1);
            lastChatOpenTime = Time.unscaledTime;
        }

        MultiplayerInteractionDatabase.ChatMessageTemplate GetMessage(int i)
        {
            ushort index = (ushort)i;
            if (index < 0 || (int)index >= MultiplayerInteractionDatabase.Instance.availableChatMessages.Length)
            {
                return null;
            }
            return MultiplayerInteractionDatabase.Instance.availableChatMessages[(int)index];
        }
    }
}
