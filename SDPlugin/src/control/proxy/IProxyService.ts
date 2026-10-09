import {Client} from "#/client/Client";

/**
 * A control-scoped surface that acts as a binding proxy to any given game client's associated service. This is
 * generally managed by the `ClientProxy` and exists to provide a stable surface for swapping between clients
 * (registration and deregistration, as necessary).
 */
export interface IProxyService {
    /**
     * Invoked when this control('s proxy) has attached to a new client.
     */
    attach(client: Client): void;

    /**
     * Invoked when this control('s proxy) has detached from a client.
     */
    detach(client: Client): void;
}
