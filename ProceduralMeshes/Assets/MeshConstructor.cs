using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets
{
    class MeshConstructor : MonoBehaviour
    {
        // All mesh generators should have these
        // these fields are custom to this mesh generator
        public bool RebuildLive = true;
        public bool Rebuild = false;
        private Stack<UnityEngine.Random.State> seeds = new Stack<UnityEngine.Random.State>(); // used for seeding rand consistently

        void Start()
        {
            Vector3 startPosition = transform.position;
            UnityEngine.Random.InitState((int)(startPosition.x + startPosition.y + startPosition.z));
            seeds.Push(UnityEngine.Random.state);
            GenerateMesh();
        }

        void Update()
        {
            if (RebuildLive)
            {
                GenerateMesh();
            }

            if (Rebuild)
            {
                GenerateMesh();
                Rebuild = false;
            }
        }

        void GenerateMesh()
        {
            Component[] cs = GetComponents(typeof(IMeshDef));
            IMeshDef meshDef = cs.Select(c => c as IMeshDef).Where(c => c != null).FirstOrDefault();

            if (meshDef == null)
            {
                Debug.LogError("Unable to find IMeshDef for MeshConstructor!");
                return;
            }

            Mesh mesh = GetComponent<MeshFilter>().mesh;
            MeshBuilder.PreBuild(mesh);
            MakeASweetAssSomething(meshDef);
            MeshBuilder.PostBuild(mesh);
        }

        void MakeASweetAssSomething(IMeshDef def)
        {
            UnityEngine.Random.state = seeds.Peek();

            float t = 0.0f;
            Vector4 prevPos = new Vector4(0, 0, 0, 1);
            int prevRing = -1;

            for (int i = 0; i <= def.MajorSubdivisions; ++i)
            {
                t = i / (float)def.MajorSubdivisions;
                Vector4 pos = def.ParameterizedPosition(t);
                Vector4 dir;

                if (def.HasParameterizedDirection)
                {
                    dir = def.ParameterizedDirection(t);
                }
                else
                {
                    if (i == 0)
                    {
                        dir = def.ParameterizedPosition(0.01f) - def.ParameterizedPosition(0.0f);
                    }
                    else
                    {
                        dir = pos - prevPos;
                    }
                }
                

                Matrix4x4 trans = Matrix4x4.Translate(pos);
                Matrix4x4 rot = Matrix4x4.Rotate(Quaternion.LookRotation(dir, dir));

                int ring = MeshBuilder.AddRing(trans * rot, def.MinorSubdivisions, def.ParameterizedRadius(t), t);
                if (prevRing != -1)
                {
                    MeshBuilder.AddBand(ring, prevRing, def.MinorSubdivisions); 
                }
                prevRing = ring;
                prevPos = pos;
            }
        }
    }
}
