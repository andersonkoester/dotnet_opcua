using DotNETOPCUA;
using Opc.Ua;
using Opc.Ua.Client;

using(OPCUA_Connection conn = new OPCUA_Connection("opc.tcp://10.0.8.20:4840")){
  Session session = await conn.Connect();

  Delegate closure = null;
  closure = delegate(NodeId node){
    Byte[] continuationPoint;
    ReferenceDescriptionCollection refs;
    session.Browse(
      null,
      null,
      ExpandedNodeId.ToNodeId(node, session.NamespaceUris),
      0u,
      BrowseDirection.Forward,
      ReferenceTypeIds.HierarchicalReferences,
      true,
      (uint)NodeClass.Variable | (uint)NodeClass.Object | (uint)NodeClass.Method,
      out continuationPoint,
      out refs);
    foreach (var rd in refs) {
      // Console.WriteLine("==> {0}, {1}, {2}", rd.DisplayName, rd.BrowseName, rd.NodeClass);
      Console.WriteLine("==> {0}, {1}", rd.DisplayName, rd.NodeId.Identifier);
      closure.DynamicInvoke(ExpandedNodeId.ToNodeId(rd.NodeId, session.NamespaceUris));
    }
  };

  closure.DynamicInvoke(ObjectIds.RootFolder);

}