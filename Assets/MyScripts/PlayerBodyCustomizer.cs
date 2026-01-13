using System.Collections.Generic;
using System.IO;
using Unity.Netcode;
using UnityEngine;

public class PlayerBodyCustomizer : NetworkBehaviour
{
    [Header("Body Variants")]
    [SerializeField] private List<GameObject> bodies = new List<GameObject>();

    private string SavePath =>
        Path.Combine(Application.persistentDataPath, "customization.json");

    private NetworkVariable<int> bodyIndexNet =
        new NetworkVariable<int>(
            1,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public override void OnNetworkSpawn()
    {
        bodyIndexNet.OnValueChanged += OnBodyIndexChanged;

        // Aplicar por si ya tiene valor
        ApplyBody(bodyIndexNet.Value);

        // SOLO el owner envía su selección
        if (IsOwner)
        {
            int localBodyIndex = LoadBodyIndex();
            SubmitBodyIndexServerRpc(localBodyIndex);
        }
    }

    public override void OnNetworkDespawn()
    {
        bodyIndexNet.OnValueChanged -= OnBodyIndexChanged;
    }

    private void OnBodyIndexChanged(int oldValue, int newValue)
    {
        ApplyBody(newValue);
    }

    private void ApplyBody(int bodyIndex)
    {
        int index = bodyIndex - 1;

        if (index < 0 || index >= bodies.Count)
            index = 0;

        for (int i = 0; i < bodies.Count; i++)
        {
            bodies[i].SetActive(i == index);
        }
    }

    [ServerRpc]
    private void SubmitBodyIndexServerRpc(int bodyIndex)
    {
        bodyIndexNet.Value = bodyIndex;
    }

    private int LoadBodyIndex()
    {
        if (!File.Exists(SavePath))
            return 1;

        string json = File.ReadAllText(SavePath);
        CustomizationData data = JsonUtility.FromJson<CustomizationData>(json);

        return data.bodyIndex;
    }
}
