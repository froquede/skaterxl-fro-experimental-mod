using GameManagement;
using ReplayEditor;
using SkaterXL.Data;
using System.Collections.Generic;
using UnityEngine;

namespace fro_mod
{
    public class RecordedFrame
    {
        public TransformInfo TransformInfo;
        public float Time;

        public RecordedFrame(TransformInfo transformInfo, float time)
        {
            TransformInfo = transformInfo;
            Time = time;
        }
    }

    public class ObjectTracker : MonoBehaviour
    {
        private float nextRecordTime;
        private List<RecordedFrame> recordedFrames;
        private Rigidbody rigidBody;
        private BoxCollider collider;
        private int BufferFrameCount;
        private Animation anim;
        private AnimationClip clip;
        private bool clipUpdated;
        private Vector3 lastPosition;
        private Quaternion lastRotation;
        float spf = 1f / 60f;
        bool kinematic = false;

        private void Awake()
        {
            recordedFrames = new List<RecordedFrame>();
            rigidBody = GetComponent<Rigidbody>();
            collider = GetComponent<BoxCollider>();
            BufferFrameCount = Mathf.RoundToInt(ReplaySettings.Instance.FPS * ReplaySettings.Instance.MaxRecordedTime);
        }

        private void Start()
        {
            lastPosition = transform.localPosition;
            lastRotation = transform.localRotation;
            kinematic = rigidBody.isKinematic;
        }

        private void RecordFrame()
        {
            if (nextRecordTime > PlayTime.time)
            {
                return;
            }
            if (nextRecordTime < PlayTime.time)
            {
                nextRecordTime = PlayTime.time + spf;
            }
            else
            {
                nextRecordTime += spf;
            }

            RecordedFrame tempRecordedFrame;
            if (recordedFrames.Count >= BufferFrameCount)
            {
                tempRecordedFrame = recordedFrames[0];
                recordedFrames.RemoveAt(0);
                tempRecordedFrame.Time = PlayTime.time;
            }
            else
            {
                tempRecordedFrame = new RecordedFrame(new TransformInfo(), PlayTime.time);
            }

            if (tempRecordedFrame.TransformInfo == null)
            {
                tempRecordedFrame.TransformInfo = new TransformInfo(transform, Space.Self);
            }

            tempRecordedFrame.TransformInfo.position = transform.localPosition;
            tempRecordedFrame.TransformInfo.rotation = transform.localRotation;
            tempRecordedFrame.Time = PlayTime.time;
            recordedFrames.Add(tempRecordedFrame);
        }

        private void Update()
        {
            if (GameStateMachine.Instance.CurrentState.GetType() == typeof(PlayState))
            {
                if (rigidBody.isKinematic)
                {
                    nextRecordTime = 0;
                    if (anim != null && anim.isPlaying)
                    {
                        anim.Stop();
                    }
                    transform.localPosition = lastPosition;
                    transform.localRotation = lastRotation;
                    if (!kinematic)
                    {
                        rigidBody.isKinematic = false;
                    }
                    collider.isTrigger = false;
                }

                clipUpdated = false;
                RecordFrame();
            }
            if (GameStateMachine.Instance.CurrentState.GetType() == typeof(ReplayState))
            {
                if (!rigidBody.isKinematic)
                {
                    lastPosition = transform.localPosition;
                    lastRotation = transform.localRotation;
                    rigidBody.isKinematic = true;
                    collider.isTrigger = true;
                }

                anim = gameObject.GetComponent<Animation>();

                if (anim == null)
                {
                    anim = gameObject.AddComponent<Animation>();
                }

                if (!clip || !clipUpdated)
                {
                    clip = new AnimationClip();
                    clip.legacy = true;
                    clip.name = $"{gameObject.name}-{gameObject.GetInstanceID()}";

                    AnimationCurve curve_pos_x = new AnimationCurve();
                    AnimationCurve curve_pos_y = new AnimationCurve();
                    AnimationCurve curve_pos_z = new AnimationCurve();
                    AnimationCurve curve_rot_x = new AnimationCurve();
                    AnimationCurve curve_rot_y = new AnimationCurve();
                    AnimationCurve curve_rot_z = new AnimationCurve();
                    AnimationCurve curve_rot_w = new AnimationCurve();

                    foreach (RecordedFrame frame in recordedFrames)
                    {
                        curve_pos_x.AddKey(frame.Time, frame.TransformInfo.position.x);
                        curve_pos_y.AddKey(frame.Time, frame.TransformInfo.position.y);
                        curve_pos_z.AddKey(frame.Time, frame.TransformInfo.position.z);
                        curve_rot_x.AddKey(frame.Time, frame.TransformInfo.rotation.x);
                        curve_rot_y.AddKey(frame.Time, frame.TransformInfo.rotation.y);
                        curve_rot_z.AddKey(frame.Time, frame.TransformInfo.rotation.z);
                        curve_rot_w.AddKey(frame.Time, frame.TransformInfo.rotation.w);
                    }

                    clip.SetCurve("", typeof(Transform), "localPosition.x", curve_pos_x);
                    clip.SetCurve("", typeof(Transform), "localPosition.y", curve_pos_y);
                    clip.SetCurve("", typeof(Transform), "localPosition.z", curve_pos_z);

                    clip.SetCurve("", typeof(Transform), "localRotation.x", curve_rot_x);
                    clip.SetCurve("", typeof(Transform), "localRotation.y", curve_rot_y);
                    clip.SetCurve("", typeof(Transform), "localRotation.z", curve_rot_z);
                    clip.SetCurve("", typeof(Transform), "localRotation.w", curve_rot_w);
                    clipUpdated = true;
                }

                anim.AddClip(clip, clip.name);
                anim.animatePhysics = true;

                var state = anim[clip.name];

                if (!anim.isPlaying && ReplayEditorController.Instance.playbackController.TimeScale != 0.0)
                {
                    anim.Play(clip.name);
                }

                state.time = ReplayEditorController.Instance.playbackController.CurrentTime;
                state.speed = ReplayEditorController.Instance.playbackController.TimeScale;
            }
        }
    }
}