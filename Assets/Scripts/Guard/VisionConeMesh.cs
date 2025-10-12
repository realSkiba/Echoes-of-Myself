using UnityEngine;

[RequireComponent(typeof(GuardFSM))] // optional
public class VisionConeMesh : MonoBehaviour {
    public float fovDegrees = 60f;
    public float range = 12f;
    [Range(8,128)] public int segments = 48;
    public float yOffset = 0.05f;
    public Material material;

    Transform child;
    MeshFilter mf;
    MeshRenderer mr;
    Mesh mesh;

    void Awake(){ EnsureChild(); Rebuild(); }
    void OnEnable(){ EnsureChild(); Rebuild(); }
    void OnValidate(){ EnsureChild(); Rebuild(); }

    void EnsureChild(){
        child = transform.Find("FOVMesh");
        if (!child){
            child = new GameObject("FOVMesh").transform;
            child.SetParent(transform, false);
        }
        mf = child.GetComponent<MeshFilter>();    if (!mf) mf = child.gameObject.AddComponent<MeshFilter>();
        mr = child.GetComponent<MeshRenderer>();  if (!mr) mr = child.gameObject.AddComponent<MeshRenderer>();
        if (mesh == null){ mesh = new Mesh { name = "FOVMeshRuntime" }; }
        mf.sharedMesh = mesh;
        if (material) mr.sharedMaterial = material;
        child.localPosition = new Vector3(0, yOffset, 0);
        child.localRotation = Quaternion.identity;
        child.localScale = Vector3.one;
    }

    [ContextMenu("Rebuild Vision Cone")]
    public void Rebuild(){
        if (mesh == null) return;
        int vCount = segments + 2;
        var verts = new Vector3[vCount];
        var tris  = new int[segments * 3];
        verts[0] = Vector3.zero;

        float half = fovDegrees * 0.5f;
        for (int i = 0; i <= segments; i++){
            float t = (float)i / segments;
            float ang = -half + t * fovDegrees;
            verts[1 + i] = Quaternion.Euler(0, ang, 0) * (Vector3.forward * range);
        }
        int tri = 0;
        for (int i = 0; i < segments; i++){
            tris[tri++] = 0; tris[tri++] = 1 + i; tris[tri++] = 2 + i;
        }
        mesh.Clear();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}
