// modified source from https://github.com/jantepya/Unity-Sprite-Voxelizer
using UnityEditor;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace Voxelizer
{
    public class VoxelMenu //: EditorWindow
    {
        private const string VOXEL_NAME_POST_FIX = "_3D";
        /*
        [MenuItem("Window/Voxelize Sprite")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow<VoxelMenu>("Voxelizer Menu");
        }
        */
        private float _scale = 1.0f;
        private bool _useMeshOptimizer = true;
        private bool _createNewGameObject = true;
        /*
        private void OnGUI()
        {
            _sprite = (Texture2D)EditorGUILayout.ObjectField("Selected Sprite", _sprite, typeof(Texture2D), true);

            string debugText = null;

            if (_sprite == null)
            {
                GUI.enabled = false;
                debugText = "Need to Select a Sprite!";
            }
            else
            {
                if (_sprite.format != TextureFormat.RGBA32)
                {
                    debugText = "For best results, set sprite compression format to RGBA32 before converting";
                }
            }

            EditorGUILayout.Space();

            _scale = (float)EditorGUILayout.FloatField("Scale", _scale);

            GUILayout.BeginVertical("HelpBox");
            _useMeshOptimizer = EditorGUILayout.Toggle("Use Mesh Optimizer", _useMeshOptimizer);
            EditorGUILayout.HelpBox("Unity's mesh optimizer optimizes the Mesh data to improve rendering performance. This operation can take a few seconds or more for complex meshes", MessageType.Info);
            GUILayout.EndVertical();
            EditorGUILayout.Space();


            _saveMesh = EditorGUILayout.Toggle("Save Mesh To File", _saveMesh);
            _saveTexture = EditorGUILayout.Toggle("Save Texture To File", _saveTexture);
            _applyColorPerVertex = EditorGUILayout.Toggle("Apply Per-vertex Colors", _applyColorPerVertex);
            _createNewGameObject = EditorGUILayout.Toggle("Add Mesh to Scene", _createNewGameObject);
            EditorGUILayout.Space();


            if (_createNewGameObject == false && _saveMesh == false)
            {
                GUI.enabled = false;
            }

            if (GUILayout.Button("Create"))
            {
                VoxelizeSprite();
            }
            GUI.enabled = true;

            if (debugText != null)
            {
                EditorGUILayout.HelpBox(debugText, MessageType.Warning);
            }
        }
        */

        private void CreateVoxelGameObject(Mesh mesh, Texture2D texture)
        {
            //var sprite3D = new GameObject(_sprite.name + VOXEL_NAME_POST_FIX);

           // var meshFilter = sprite3D.AddComponent<MeshFilter>();
            //meshFilter.sharedMesh = mesh;

            //var meshRenderer = sprite3D.AddComponent<MeshRenderer>();

            if (texture != null)
            {
                var material = new Material(Shader.Find("Standard"));
                material.SetTexture("_MainTex", texture);
                //meshRenderer.sharedMaterial = material;
            }
        }

        private void SaveMeshToFile(Mesh mesh)
        {
            /*
            string path = EditorUtility.SaveFilePanel("Save mesh to folder", "Assets/", mesh.name, "asset");

            path = FileUtil.GetProjectRelativePath(path);

            if (string.IsNullOrEmpty(path) == false)
            {
                AssetDatabase.CreateAsset(mesh, path);
                AssetDatabase.SaveAssets();
            }
            else
            {
                Debug.LogWarning("[Voxelizer] Mesh Export failed: invalid path");
            }
            */
        }

        private void SaveTextureToFile(Texture2D texture)
        {
            /*
            texture.name = _sprite.name + VOXEL_NAME_POST_FIX;
            string path = EditorUtility.SaveFilePanel("Save texture to folder", "Assets/", texture.name, "PNG");

            path = FileUtil.GetProjectRelativePath(path);

            if (string.IsNullOrEmpty(path) == false)
            {
                byte[] _bytes = texture.EncodeToPNG();
                System.IO.File.WriteAllBytes(path, _bytes);
            }
            else
            {
                Debug.LogWarning("[Voxelizer] Texture Export failed: invalid path");
            }
            */
        }

        //Read texture independent of Read/Write enabled on the sprite
        public static Texture2D ReadTexture(Texture2D texture)
        {
            Texture2D duplicate = duplicateTexture(texture);
            Texture2D newTexture = new Texture2D(duplicate.width, duplicate.height, duplicate.format, false);
            newTexture.LoadRawTextureData(duplicate.GetRawTextureData());
            newTexture.Apply();
            return newTexture;
        }

        // found on https://stackoverflow.com/questions/44733841/how-to-make-texture2d-readable-via-script
        public static Texture2D duplicateTexture(Texture2D source)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                        source.width,
                        source.height,
                        0,
                        RenderTextureFormat.Default,
                        RenderTextureReadWrite.Linear);

            Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableText = new Texture2D(source.width, source.height);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }
    }
}
