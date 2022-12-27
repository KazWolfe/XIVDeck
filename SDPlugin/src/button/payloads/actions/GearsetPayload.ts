export type GearsetPayload = {
    /**
     * Request a specific glamour plate be used when switching to the specified gearset.
     * 
     * Value must be between 1-20, but this is enforced at the game level.
     */
    glamourPlateId?: number
}