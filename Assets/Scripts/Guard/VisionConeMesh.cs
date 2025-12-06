using UnityEngine;

[ExecuteAlways] // so it updates in Edit mode too
public class VisionConeMesh : MonoBehaviour
{
    [Header("Defaults (used if no guard script found)")]
    public float fovDegrees = 60f;
    public float range = 12f;
    [Range(8,128)] public int segments = 48;
    public float yOffset = 0.05f;
    public Material material;
    public bool followEye = true;       // if guard exposes an 'eye' Transform, use it
    public bool matchGuardValues = true;// read fov/range from guard each frame

    // Optional: for manual hookup if your guard exposes an eye
    public Transform explicitEye;

    // cached
    Transform child;
    MeshFilter mf;
    MeshRenderer mr;
    Mesh mesh;

    // Optional soft-link to either guard script
    object guardRef;
    System.Reflection.FieldInfo eyeField, fovField, rangeField;

    void OnEnable(){ EnsureChild(); TryBindGuard(); Rebuild(); }
    void Awake(){ EnsureChild(); TryBindGuard(); Rebuild(); }
    void OnValidate(){ EnsureChild(); TryBindGuard(); Rebuild(); }

    void Update(){
        // In Play or Edit, track guard fields if available
        if (matchGuardValues && guardRef != null){
            if (fovField != null)  fovDegrees = (float)fovField.GetValue(guardRef);
            if (rangeField != null) range = (float)rangeField.GetValue(guardRef);
        }

        // keep mesh child positioned at the eye (or root) every frame
        Transform eyeT = GetEyeTransform();
        if (eyeT != null){
            child.position = eyeT.position + Vector3.up * yOffset;  // small lift above floor
            child.rotation = Quaternion.Euler(0f, eyeT.eulerAngles.y, 0f);
        } else {
            child.localPosition = new Vector3(0, yOffset, 0);
            child.localRotation = Quaternion.identity;
        }
    }

    void EnsureChild(){
        if (child == null){
            var t = transform.Find("FOVMesh");
            child = t ? t : new GameObject("FOVMesh").transform;
            child.SetParent(transform, false);
        }
        if (!mf) mf = child.GetComponent<MeshFilter>() ?? child.gameObject.AddComponent<MeshFilter>();
        if (!mr) mr = child.GetComponent<MeshRenderer>() ?? child.gameObject.AddComponent<MeshRenderer>();
        if (mesh == null) mesh = new Mesh { name = "FOVMeshRuntime" };
        mf.sharedMesh = mesh;
        if (material) mr.sharedMaterial = material;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;
    }

    Transform GetEyeTransform(){
        if (explicitEye) return explicitEye;

        if (followEye && guardRef != null && eyeField != null){
            var eye = eyeField.GetValue(guardRef) as Transform;
            if (eye) return eye;
        }
        return transform; // fallback: use guard root
    }

    void TryBindGuard(){
        guardRef = null; eyeField = fovField = rangeField = null;

        // Try GuardFSM_Net first
        var net = GetComponent("GuardFSM_Net");
        if (net){
            guardRef = net;
            var t = net.GetType();
            eyeField   = t.GetField("eye");
            fovField   = t.GetField("fovDegrees");
            rangeField = t.GetField("sightDistance");
            return;
        }
        // Fallback to old GuardFSM (non-network)
        var old = GetComponent("GuardFSM");
        if (old){
            guardRef = old;
            var t = old.GetType();
            eyeField   = t.GetField("eye");
            fovField   = t.GetField("fovDegrees");
            rangeField = t.GetField("sightDistance");
        }
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
