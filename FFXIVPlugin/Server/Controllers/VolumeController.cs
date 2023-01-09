using System;
using System.Collections.Generic;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using XIVDeck.FFXIVPlugin.Game.Data;
using XIVDeck.FFXIVPlugin.Server.Helpers;
using XIVDeck.FFXIVPlugin.Server.Types;

namespace XIVDeck.FFXIVPlugin.Server.Controllers; 

[ApiController("/volume")]
public class VolumeController : WebApiController {
    [Route(HttpVerbs.Get, "/")]
    public Dictionary<SoundChannel, SerializableVolumeMessage> GetAllChannels() {
        Dictionary<SoundChannel, SerializableVolumeMessage> reply = new();

        foreach (var channel in Enum.GetValues<SoundChannel>()) {
            reply[channel] = new SerializableVolumeMessage(channel);
        }

        return reply;
    }

    [Route(HttpVerbs.Get, "/{channel}")]
    public SerializableVolumeMessage GetChannel(SoundChannel channel) {
        return new SerializableVolumeMessage(channel);
    }

    [Route(HttpVerbs.Put, "/{channel}")]
    public SerializableVolumeMessage SetChannel(SoundChannel channel, [NJsonData] SerializableVolumeMessage body) {
        body.ApplyToChannel(channel);

        return new SerializableVolumeMessage(channel);
    }
}