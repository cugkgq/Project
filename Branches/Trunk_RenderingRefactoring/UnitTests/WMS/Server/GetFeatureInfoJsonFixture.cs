using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap.Layers;
using SharpMap.Web.Wms.Server;
using SharpMap.Web.Wms.Server.Handlers;

namespace UnitTests.WMS.Server
{
    public class GetFeatureInfoJsonFixture : AbstractFixture
    {
        [Test]
        public void request_generates_valid_json()
        {
            MockRepository mocks = new MockRepository();
            IContextRequest req = mocks.StrictMock<IContextRequest>();
            With.Mocks(mocks)
                .Expecting(() =>
                {
                    Expect.Call(req.GetParam("VERSION")).Return("1.3.0");
                    Expect.Call(req.GetParam("LAYERS")).Return("poly_landmarks,tiger_roads,poi");
                    Expect.Call(req.GetParam("STYLES")).Return("");
                    Expect.Call(req.GetParam("CRS")).Return("EPSG:4326");
                    Expect.Call(req.GetParam("BBOX")).Return("40.68,-74.02,40.69,-74.01");
                    Expect.Call(req.GetParam("WIDTH")).Return("256");
                    Expect.Call(req.GetParam("HEIGHT")).Return("256");
                    Expect.Call(req.GetParam("FORMAT")).Return("image/png");
                    Expect.Call(req.GetParam("CQL_FILTER")).Return(null);
                    Expect.Call(req.GetParam("QUERY_LAYERS")).Return("poly_landmarks");
                    Expect.Call(req.GetParam("INFO_FORMAT")).Return("text/json");
                    Expect.Call(req.GetParam("X")).Return(null);
                    Expect.Call(req.GetParam("I")).Return("128");
                    Expect.Call(req.GetParam("Y")).Return(null);
                    Expect.Call(req.GetParam("J")).Return("128");
                    Expect.Call(req.GetParam("FEATURE_COUNT")).Return("10");
                })
                .Verify(() =>
                {
                    IHandler handler = new GetFeatureInfoJson(Desc);
                    IHandlerResponse resp = handler.Handle(Map, req);
                    Assert.That(resp, Is.Not.Null);
                    Assert.IsInstanceOf<GetFeatureInfoResponseJson>(resp);
                    GetFeatureInfoResponseJson json = (GetFeatureInfoResponseJson)resp;
                    string contentType = json.ContentType;
                    Assert.That(contentType, Is.Not.Null);
                    Assert.That(contentType, Is.EqualTo("text/json"));
                    string charset = json.Charset;
                    Assert.That(charset, Is.Not.Null);
                    Assert.That(charset, Is.EqualTo("utf-8"));

                    JContainer c = (JContainer) JsonConvert.DeserializeObject(json.Response);                    
                    JToken token = c.First;
                    Assert.That(token, Is.InstanceOf<JProperty>());
                    JProperty type = (JProperty)token;
                    Assert.That(type.Name, Is.EqualTo("type"));
                    Assert.That(type.Value.Value<string>(), Is.EqualTo("FeatureCollection"));
                    token = token.Next;
                    Assert.That(token, Is.InstanceOf<JProperty>());
                    JProperty features = (JProperty)token;
                    Assert.That(features.Name, Is.EqualTo("features"));
                    Assert.That(features.Value, Is.InstanceOf<JArray>());
                });
        }

        [Test]
        public void no_queryable_layer()
        {
            MockRepository mocks = new MockRepository();
            IContextRequest req = mocks.StrictMock<IContextRequest>();
            With.Mocks(mocks)
                .Expecting(() =>
                {
                    Expect.Call(req.GetParam("VERSION")).Return("1.3.0");
                    Expect.Call(req.GetParam("LAYERS")).Return("poly_landmarks,tiger_roads,poi");
                    Expect.Call(req.GetParam("STYLES")).Return("");
                    Expect.Call(req.GetParam("CRS")).Return("EPSG:4326");
                    Expect.Call(req.GetParam("BBOX")).Return("40.68,-74.02,40.69,-74.01");
                    Expect.Call(req.GetParam("WIDTH")).Return("256");
                    Expect.Call(req.GetParam("HEIGHT")).Return("256");
                    Expect.Call(req.GetParam("FORMAT")).Return("image/png");
                    Expect.Call(req.GetParam("CQL_FILTER")).Return(null);
                    Expect.Call(req.GetParam("QUERY_LAYERS")).Return("poly_landmarks");
                    Expect.Call(req.GetParam("INFO_FORMAT")).Return("text/json");
                    Expect.Call(req.GetParam("X")).Return(null);
                    Expect.Call(req.GetParam("I")).Return("128");
                    Expect.Call(req.GetParam("Y")).Return(null);
                    Expect.Call(req.GetParam("J")).Return("128");
                    Expect.Call(req.GetParam("FEATURE_COUNT")).Return("10");
                })
                .Verify(() =>
                {
                    foreach (ILayer layer in Map.Layers)
                        ((VectorLayer) layer).IsQueryEnabled = false;

                    IHandler handler = new GetFeatureInfoJson(Desc);
                    IHandlerResponse resp = handler.Handle(Map, req);
                    Assert.That(resp, Is.Not.Null);
                    Assert.IsInstanceOf<GetFeatureInfoResponseJson>(resp);
                    GetFeatureInfoResponseJson json = (GetFeatureInfoResponseJson)resp;
                    string contentType = json.ContentType;
                    Assert.That(contentType, Is.Not.Null);
                    Assert.That(contentType, Is.EqualTo("text/json"));
                    string charset = json.Charset;
                    Assert.That(charset, Is.Not.Null);
                    Assert.That(charset, Is.EqualTo("utf-8"));
                    Assert.That(json.Response, Is.EqualTo("{\"type\":\"FeatureCollection\",\"features\":[]}"));
                });
        }
    }
}