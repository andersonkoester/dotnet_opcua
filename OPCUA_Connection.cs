using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;

namespace DotNETOPCUA;

public class OPCUA_Connection : IDisposable{

  public SessionReconnectHandler? reconnectHandler {get; set;}
  public const int ReconnectPeriod = 10;
  public Session? session {get;set;}
  public ReferenceDescriptionCollection? references {get;set;}

  public ApplicationConfiguration? config {get;set;}

  private ConfiguredEndpoint endpoint {get;set;}

  public OPCUA_Connection(string server_endpoint){

    config = new ApplicationConfiguration{
      ApplicationName = "Teste de Aplicação OCPUA",
      ApplicationType = ApplicationType.Client,
      TransportQuotas = new TransportQuotas{
        OperationTimeout = 600000,
        MaxStringLength = 1048576,
        MaxByteStringLength = 4194304,
        MaxArrayLength = 65535,
        MaxMessageSize = 4194304,
        MaxBufferSize = 65535,
        ChannelLifetime = 300000,
        SecurityTokenLifetime = 3600000
      },
      ClientConfiguration = new ClientConfiguration{
        DefaultSessionTimeout = 600000,
        MinSubscriptionLifetime = 10000
      }
    };

    var selectedEndpoint = CoreClientUtils.SelectEndpoint(server_endpoint, false, 15000);
    var endpointConfiguration = EndpointConfiguration.Create(config);
    endpoint = new ConfiguredEndpoint(null, selectedEndpoint, endpointConfiguration);
    
  }

  public async Task<Session> Connect(){
    session = await Session.Create(config, endpoint, false, "OPC UA Console Client", 60000,
                  new UserIdentity(new AnonymousIdentityToken()), null);

    session.KeepAlive += Client_KeepAlive;

    return session;
  }

  public void Dispose(){
    session.Dispose();
  }

  private void Client_ReconnectComplete(object sender, EventArgs e){
    if (!Object.ReferenceEquals(sender, reconnectHandler))
      return;

    session = reconnectHandler.Session;
    reconnectHandler.Dispose();
    reconnectHandler = null;
  }

  private void Client_KeepAlive(Session sender, KeepAliveEventArgs e){
    if (e.Status != null && ServiceResult.IsNotGood(e.Status)){
      if (reconnectHandler == null){
          reconnectHandler = new SessionReconnectHandler();
          reconnectHandler.BeginReconnect(sender, ReconnectPeriod * 1000, Client_ReconnectComplete);
      }
    }
  }

}