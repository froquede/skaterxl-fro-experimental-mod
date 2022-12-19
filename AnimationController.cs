using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RealisticEyeMovements;
using RootMotion.FinalIK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityModManagerNet;

namespace fro_mod
{
    class AnimationJSON
    {
        public float duration;
        public float[] times;
        public AnimationJSONParts parts;

        public AnimationJSON(float duration, float[] times, AnimationJSONParts parts)
        {
            this.duration = duration;
            this.times = times;
            this.parts = parts;
        }

        public override string ToString()
        {
            return this.duration + " " + this.times.Length ;
        }
    }

    class AnimationJSONParts
    {
        public AnimationJSONPart Hips { get; set; }
        public AnimationJSONPart Spine { get; set; }
        public AnimationJSONPart Spine1 { get; set; }
        public AnimationJSONPart Spine2 { get; set; }
        public AnimationJSONPart Neck { get; set; }
        public AnimationJSONPart Head { get; set; }
        public AnimationJSONPart HeadTop_End { get; set; }
        public AnimationJSONPart LeftShoulder { get; set; }
        public AnimationJSONPart LeftArm { get; set; }
        public AnimationJSONPart LeftForeArm { get; set; }
        public AnimationJSONPart LeftHand { get; set; }
        public AnimationJSONPart LeftHandThumb1 { get; set; }
        public AnimationJSONPart LeftHandThumb2 { get; set; }
        public AnimationJSONPart LeftHandThumb3 { get; set; }
        public AnimationJSONPart LeftHandThumb4 { get; set; }
        public AnimationJSONPart LeftHandIndex1 { get; set; }
        public AnimationJSONPart LeftHandIndex2 { get; set; }
        public AnimationJSONPart LeftHandIndex3 { get; set; }
        public AnimationJSONPart LeftHandIndex4 { get; set; }
        public AnimationJSONPart LeftHandMiddle1 { get; set; }
        public AnimationJSONPart LeftHandMiddle2 { get; set; }
        public AnimationJSONPart LeftHandMiddle3 { get; set; }
        public AnimationJSONPart LeftHandMiddle4 { get; set; }
        public AnimationJSONPart LeftHandRing1 { get; set; }
        public AnimationJSONPart LeftHandRing2 { get; set; }
        public AnimationJSONPart LeftHandRing3 { get; set; }
        public AnimationJSONPart LeftHandRing4 { get; set; }
        public AnimationJSONPart LeftHandPinky1 { get; set; }
        public AnimationJSONPart LeftHandPinky2 { get; set; }
        public AnimationJSONPart LeftHandPinky3 { get; set; }
        public AnimationJSONPart LeftHandPinky4 { get; set; }
        public AnimationJSONPart RightShoulder { get; set; }
        public AnimationJSONPart RightArm { get; set; }
        public AnimationJSONPart RightForeArm { get; set; }
        public AnimationJSONPart RightHand { get; set; }
        public AnimationJSONPart RightHandThumb1 { get; set; }
        public AnimationJSONPart RightHandThumb2 { get; set; }
        public AnimationJSONPart RightHandThumb3 { get; set; }
        public AnimationJSONPart RightHandThumb4 { get; set; }
        public AnimationJSONPart RightHandIndex1 { get; set; }
        public AnimationJSONPart RightHandIndex2 { get; set; }
        public AnimationJSONPart RightHandIndex3 { get; set; }
        public AnimationJSONPart RightHandIndex4 { get; set; }
        public AnimationJSONPart RightHandMiddle1 { get; set; }
        public AnimationJSONPart RightHandMiddle2 { get; set; }
        public AnimationJSONPart RightHandMiddle3 { get; set; }
        public AnimationJSONPart RightHandMiddle4 { get; set; }
        public AnimationJSONPart RightHandRing1 { get; set; }
        public AnimationJSONPart RightHandRing2 { get; set; }
        public AnimationJSONPart RightHandRing3 { get; set; }
        public AnimationJSONPart RightHandRing4 { get; set; }
        public AnimationJSONPart RightHandPinky1 { get; set; }
        public AnimationJSONPart RightHandPinky2 { get; set; }
        public AnimationJSONPart RightHandPinky3 { get; set; }
        public AnimationJSONPart RightHandPinky4 { get; set; }
        public AnimationJSONPart LeftUpLeg { get; set; }
        public AnimationJSONPart LeftLeg { get; set; }
        public AnimationJSONPart LeftFoot { get; set; }
        public AnimationJSONPart LeftToeBase { get; set; }
        public AnimationJSONPart LeftToe_End { get; set; }
        public AnimationJSONPart RightUpLeg { get; set; }
        public AnimationJSONPart RightLeg { get; set; }
        public AnimationJSONPart RightFoot { get; set; }
        public AnimationJSONPart RightToeBase { get; set; }
        public AnimationJSONPart RightToe_End { get; set; }
    }

    class AnimationJSONPart
    {
        public float[][] position, quaternion;

        public AnimationJSONPart(float[][] position, float[][] quaternion)
        {
            this.position = position;
            this.quaternion = quaternion;
        }

        public override string ToString()
        {
            return this.position.Length + " " + this.quaternion.Length;
        }
    }

    public class AnimController : MonoBehaviour
    {
        AnimationJSON walking;
        List<GameObject> debug = new List<GameObject>(17);
        string[] bones = new string[] { "Spine", "Spine1", "Spine2", "Neck", "Head", "LeftArm", "LeftForeArm", "LeftHand", "RightArm", "RightForeArm", "RightHand", "LeftUpLeg", "LeftLeg", "LeftFoot", "RightUpLeg", "RightLeg", "RightFoot" };
        GameObject fakeSkater;

        void Start()
        {
            fakeSkater = Instantiate(PlayerController.Instance.skaterController.skaterTransform.gameObject);
            Destroy(fakeSkater.GetComponent<Animator>());
            Destroy(fakeSkater.GetComponent<Rigidbody>());
            Destroy(fakeSkater.GetComponent<SkaterController>());
            Destroy(fakeSkater.GetComponent<AnimationController>());
            Destroy(fakeSkater.GetComponent<CoMDisplacement>());
            Destroy(fakeSkater.GetComponent<Respawn>());
            Destroy(fakeSkater.GetComponent<IKController>());
            Destroy(fakeSkater.GetComponent<Bail>());
            Destroy(fakeSkater.GetComponent<HeadIK>());
            Destroy(fakeSkater.GetComponent<GestureAnimationController>());
            Destroy(fakeSkater.GetComponent<FullBodyBipedIK>());
            Destroy(fakeSkater.GetComponent<LookAtIK>());
            Destroy(fakeSkater.GetComponent<EyeAndHeadAnimator>());
            Destroy(fakeSkater.GetComponent<LookTargetController>());

            getParts();

            Material white = new Material(Shader.Find("HDRP/Lit"));

            for (int i = 0; i < 17; i++)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.localScale = new Vector3(.075f, .075f, .075f);
                cube.GetComponent<BoxCollider>().enabled = false;
                cube.GetComponent<MeshRenderer>().material = white;
                cube.transform.name = bones[i];
                debug.Add(cube);
            }

            string fileName = Path.Combine(Main.modEntry.Path, "walking.json");
            string json;
            if (File.Exists(fileName))
            {
                json = File.ReadAllText(fileName);
            }
            else
            {
                return;
            }

            JObject animation = JObject.Parse(json);
            AnimationJSONParts parts = new AnimationJSONParts();

            foreach(string part in bones)
            {
                try
                {
                    Type type = typeof(AnimationJSONParts);
                    var property = type.GetProperty(part);                
                    AnimationJSONPart new_part = new AnimationJSONPart(JsonConvert.DeserializeObject<float[][]>(animation["parts"][part]["position"].ToString()), JsonConvert.DeserializeObject<float[][]>(animation["parts"][part]["quaternion"].ToString()));
                    property.SetValue(parts, new_part);
                }
                catch { }
            }

            walking = new AnimationJSON((float)animation["duration"], Newtonsoft.Json.JsonConvert.DeserializeObject<float[]>(animation["times"].ToString()), parts);
        }

        float animTime = 0f;
        void FixedUpdate()
        {
            int index = 0;
            /*PlayerController.Instance.ikController.SetIKRigidbodyKinematic(true);
            PlayerController.Instance.ikController.OnOffIK(0);
            PlayerController.Instance.ikController.enabled = false;
            PlayerController.Instance.headIk.enabled = false;
            PlayerController.Instance.AnimController.ikAnim.enabled = false;
            PlayerController.Instance.AnimController.skaterAnim.enabled = false;*/

            fakeSkater.transform.position = PlayerController.Instance.skaterController.skaterTransform.position;
            fakeSkater.transform.rotation = Quaternion.identity;
            fakeSkater.transform.Rotate(0, 90f, 0, Space.Self);

            for (int i = 0; i < walking.times.Length; i++)
            {
                index = i;
                if (walking.times[i] >= animTime) break;
            }

            foreach (string part in bones)
            {
                Transform tpart = getPart(part);
                if(tpart)
                {
                    try
                    {
                        Type type = typeof(AnimationJSONParts);
                        var property = type.GetProperty(part);
                        AnimationJSONPart apart = (AnimationJSONPart)property.GetValue(walking.parts, null);

                        GameObject copy = new GameObject();
                        copy.transform.position = PlayerController.Instance.boardController.boardTransform.position;
                        copy.transform.rotation = PlayerController.Instance.boardController.boardTransform.rotation;

                        copy.transform.Translate(new Vector3(apart.position[index][0], apart.position[index][1], apart.position[index][2]));

                        /*tpart.position = PlayerController.Instance.boardController.boardTransform.position + new Vector3(apart.position[index][0], apart.position[index][1], apart.position[index][2]);
                        tpart.rotation = new Quaternion(apart.quaternion[index][0], apart.quaternion[index][1], apart.quaternion[index][2], apart.quaternion[index][3]);*/

                        tpart.position = copy.transform.position;
                        tpart.rotation = new Quaternion(-apart.quaternion[index][0], -apart.quaternion[index][1], -apart.quaternion[index][2], -apart.quaternion[index][3]); ;

                        Destroy(copy);
                    }
                    catch { }
                }
            }
            animTime += Time.fixedDeltaTime;

            if (animTime > walking.duration) animTime = 0;
        }

        Transform getPart(string id)
        {
            if (id == "Hips") return pelvis;
            if (id == "Spine") return spine;
            if (id == "Spine1") return spine1;
            if (id == "Spine2") return spine2;
            if (id == "Neck") return neck;
            if (id == "Head") return head;
            if (id == "LeftArm") return left_arm;
            if (id == "LeftForeArm") return left_forearm;
            if (id == "LeftHand") return left_hand;
            if (id == "RightArm") return right_arm;
            if (id == "RightForeArm") return right_forearm;
            if (id == "RightHand") return right_hand;
            if (id == "LeftUpLeg") return left_upleg;
            if (id == "LeftLeg") return left_leg;
            if (id == "LeftFoot") return left_foot;
            if (id == "RightUpLeg") return right_upleg;
            if (id == "RightLeg") return right_leg;
            if (id == "RightFoot") return right_foot;

            return null;
        }

        Transform getPartDebug(string id)
        {
            if (id == "Spine") return debug[0].transform;
            if (id == "Spine1") return debug[1].transform;
            if (id == "Spine2") return debug[2].transform;
            if (id == "Neck") return debug[3].transform;
            if (id == "Head") return debug[4].transform;
            if (id == "LeftArm") return debug[5].transform;
            if (id == "LeftForeArm") return debug[6].transform;
            if (id == "LeftHand") return debug[7].transform;
            if (id == "RightArm") return debug[8].transform;
            if (id == "RightForeArm") return debug[9].transform;
            if (id == "RightHand") return debug[10].transform;
            if (id == "LeftUpLeg") return debug[11].transform;
            if (id == "LeftLeg") return debug[12].transform;
            if (id == "LeftFoot") return debug[13].transform;
            if (id == "RightUpLeg") return debug[14].transform;
            if (id == "RightLeg") return debug[15].transform;
            if (id == "RightFoot") return debug[16].transform;

            return null;
        }

        Transform pelvis, spine, spine1, spine2, neck, head, left_arm, left_forearm, left_hand, right_arm, right_forearm, right_hand, left_upleg, left_leg, left_foot, right_upleg, right_leg, right_foot;
        void getParts()
        {
            Transform parent = fakeSkater.transform;
            Transform joints = parent.Find("Skater_Joints");

            pelvis = joints.FindChildRecursively("Skater_pelvis");
            spine = joints.FindChildRecursively("Skater_Spine");
            spine1 = joints.FindChildRecursively("Skater_Spine1");
            spine2 = joints.FindChildRecursively("Skater_Spine2");
            neck = joints.FindChildRecursively("Skater_Neck");
            head = joints.FindChildRecursively("Skater_Head");
            left_arm = joints.FindChildRecursively("Skater_Arm_l");
            left_forearm = joints.FindChildRecursively("Skater_ForeArm_l");
            left_hand = joints.FindChildRecursively("Skater_hand_l");
            right_arm = joints.FindChildRecursively("Skater_Arm_r");
            right_forearm = joints.FindChildRecursively("Skater_ForeArm_r");
            right_hand = joints.FindChildRecursively("Skater_hand_r");
            left_upleg = joints.FindChildRecursively("Skater_UpLeg_l");
            left_leg = joints.FindChildRecursively("Skater_Leg_l");
            left_foot = joints.FindChildRecursively("Skater_foot_l");
            right_upleg = joints.FindChildRecursively("Skater_UpLeg_r");
            right_leg = joints.FindChildRecursively("Skater_Leg_r");
            right_foot = joints.FindChildRecursively("Skater_foot_r");
        }
    }    
}

