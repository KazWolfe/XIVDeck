import {DOMParser, XMLSerializer} from "@xmldom/xmldom";
import type {Document, Element} from "@xmldom/xmldom";
import {fastClone} from "../util/DomUtil";
import {ActionCostType} from "../rpc/messages/ActionAppearance";
import hotbarSlotTemplate from "../../assets/templates/HotbarSlot.svg";

export interface HotbarSlotCost {
    text: string;
    type: ActionCostType;
    position: "left" | "right";
}

export interface HotbarSlotRenderOptions {
    iconBase64: string;
    cooldownElapsedPercent?: number;
    cooldownSecondsRemaining?: number;
    cost?: HotbarSlotCost;
    chargeCount?: number;
    rechargeTimerDonutPercent?: number;
    rechargeTimerSweepPercent?: number;
    typeIcon?: string;
    highlighted?: boolean;
}

const ICON_ID = "Icon";
const RECAST_TIMER_GROUP_ID = "RecastTimer";
const RECAST_TIMER_MASK_PATH_ID = "RecastTimerMaskPath";
const RECAST_TIMER_WEDGE_MASK_PATH_ID = "RecastTimerWedgeMaskPath";
const RECAST_TIMER_EDGE_GLOW_ID = "RecastTimerEdgeGlow";
const RECAST_TIMER_EDGE_CRISP_ID = "RecastTimerEdgeCrisp";
const HIGHLIGHT_GROUP_ID = "ComboHighlight";
const LEFT_COST_GROUP_ID = "LeftCost";
const RIGHT_COST_GROUP_ID = "RightCost";
const CHARGE_COUNT_GROUP_ID = "ChargeCount";
const CHARGE_COUNT_SHADOW_ID = "ChargeCountShadow";
const CHARGE_COUNT_TEXT_ID = "ChargeCountText";
const CHARGE_COUNT_ZERO_GROUP_ID = "ChargeCountZero";
const RECHARGE_TIMER_DONUT_GROUP_ID = "RechargeTimerDonut";
const RECHARGE_TIMER_SWEEP_GROUP_ID = "RechargeTimerSweep";
const RECHARGE_TIMER_SWEEP_FILL_ID = "RechargeTimerSweepFill";
const RECHARGE_TIMER_SWEEP_EDGE_GROUP_ID = "RechargeTimerSweepEdge";
const TYPE_ICON_ID = "TypeIcon";

const COOLDOWN_CENTER = 40;
const COOLDOWN_CIRCLE_RADIUS = 39;
const RECHARGE_TIMER_SWEEP_RADIUS = 60;
const RECHARGE_TIMER_DONUT_CIRCUMFERENCE = 2 * Math.PI * 32;

interface CostColor {
    base: string;
    glow: string;
}

const COST_TYPE_COLORS: Record<ActionCostType, CostColor> = {
    [ActionCostType.Default]: {base: "#ffffff", glow: "#333333"},   // White
    [ActionCostType.Health]: {base: "#c9ffe4", glow: "#0a5f24"},    // Green (HP)
    [ActionCostType.Magic]: {base: "#ffd9fa", glow: "#985090"},     // Light Pink (MP)
    [ActionCostType.Tactical]: {base: "#ffe6bb", glow: "#7c1d00"},  // Orange
    [ActionCostType.Crafting]: {base: "#e5c4ff", glow: "#b71b7e"},  // Pink (DoH - CP)
    [ActionCostType.Gathering]: {base: "#fff1d4", glow: "#7f7c1d"}, // Yellow (DoL - GP)
    [ActionCostType.JobGauge]: {base: "#dcffff", glow: "#3040ec"},  // Blue (Job Gauge)
    [ActionCostType.Ceruleum]: {base: "#fdffd9", glow: "#958a12"},  // Bright Yellow (Rival Wings - CE)
};

export class HotbarSlotRenderer {
    private readonly template: Document;

    constructor(template: string = hotbarSlotTemplate) {
        this.template = new DOMParser().parseFromString(template, "image/svg+xml");
    }

    render(options: HotbarSlotRenderOptions): string {
        const slot = new HotbarSlotDocument(this.template);

        slot.setIcon(options.iconBase64);
        slot.setRecastTimer(options.cooldownElapsedPercent ?? 0);
        slot.setVisible(HIGHLIGHT_GROUP_ID, options.highlighted ?? false);
        slot.setCost(options.cost, options.cooldownSecondsRemaining);
        slot.setChargeCount(options.chargeCount);
        slot.setRechargeTimerDonut(options.rechargeTimerDonutPercent);
        slot.setRechargeTimerSweep(options.rechargeTimerSweepPercent);
        slot.setTypeIcon(options.typeIcon);

        return toSvgDataUrl(new XMLSerializer().serializeToString(slot.document));
    }
}

/** A single render's copy of the hotbar slot template. */
class HotbarSlotDocument {
    readonly document: Document;
    private readonly ids: ReadonlyMap<string, Element>;

    constructor(template: Document) {
        this.document = fastClone(template);
        this.ids = this.indexById(this.document);
    }

    setIcon(iconBase64: string): void {
        const image = this.ids.get(ICON_ID);
        image?.setAttribute("xlink:href", `data:image/png;base64,${iconBase64}`);
    }

    setRecastTimer(elapsedPercent: number): void {
        const active = elapsedPercent > 0 && elapsedPercent < 100;
        this.setVisible(RECAST_TIMER_GROUP_ID, active);
        if (!active) return;

        const maskPath = this.ids.get(RECAST_TIMER_MASK_PATH_ID);
        maskPath?.setAttribute("d", this.buildWedgeMaskPath(elapsedPercent));

        const wedgePath = this.buildWedgePath(elapsedPercent, COOLDOWN_CIRCLE_RADIUS);

        const wedgeMaskPath = this.ids.get(RECAST_TIMER_WEDGE_MASK_PATH_ID);
        wedgeMaskPath?.setAttribute("d", wedgePath);

        const glow = this.ids.get(RECAST_TIMER_EDGE_GLOW_ID);
        glow?.setAttribute("d", wedgePath);

        const crisp = this.ids.get(RECAST_TIMER_EDGE_CRISP_ID);
        crisp?.setAttribute("d", wedgePath);
    }

    setRechargeTimerSweep(progressPercent: number | undefined): void {
        const active = progressPercent != null && progressPercent > 0 && progressPercent < 100;
        this.setVisible(RECHARGE_TIMER_SWEEP_GROUP_ID, active);
        if (!active) return;

        const fill = this.ids.get(RECHARGE_TIMER_SWEEP_FILL_ID);
        fill?.setAttribute("d", this.buildWedgePath(progressPercent!, RECHARGE_TIMER_SWEEP_RADIUS));

        const sweepDeg = (progressPercent! / 100) * 360;
        const edgeGroup = this.ids.get(RECHARGE_TIMER_SWEEP_EDGE_GROUP_ID);
        edgeGroup?.setAttribute("transform", `rotate(${sweepDeg.toFixed(2)} ${COOLDOWN_CENTER} ${COOLDOWN_CENTER})`);
    }

    private buildWedgeMaskPath(elapsedPercent: number): string {
        const square = `M0,0 H80 V80 H0 Z`;
        const wedge = this.buildWedgePath(elapsedPercent, COOLDOWN_CIRCLE_RADIUS);

        return `${square} ${wedge}`;
    }

    private buildWedgePath(elapsedPercent: number, r: number): string {
        const cx = COOLDOWN_CENTER;
        const cy = COOLDOWN_CENTER;

        const sweepDeg = (elapsedPercent / 100) * 360;
        const largeArc = sweepDeg > 180 ? 1 : 0;
        const start = this.polarPoint(cx, cy, r, 0);
        const end = this.polarPoint(cx, cy, r, sweepDeg);

        return `M${cx},${cy} L${start.x},${start.y} A${r},${r} 0 ${largeArc},1 ${end.x},${end.y} Z`;
    }

    private polarPoint(cx: number, cy: number, r: number, angleDeg: number): { x: number; y: number } {
        const rad = (Math.PI / 180) * angleDeg;
        return {x: cx + r * Math.sin(rad), y: cy - r * Math.cos(rad)};
    }

    setCost(cost: HotbarSlotCost | undefined, cooldownSecondsRemaining: number | undefined): void {
        this.setVisible(LEFT_COST_GROUP_ID, false);
        this.setVisible(RIGHT_COST_GROUP_ID, false);

        if (cost) {
            const groupId = cost.position === "left" ? LEFT_COST_GROUP_ID : RIGHT_COST_GROUP_ID;
            this.setCostGroupContent(groupId, cost.text, this.getCostColor(cost.type));
        }

        // cooldowns will always override "standard" display
        if (cooldownSecondsRemaining != null) {
            this.setCostGroupContent(LEFT_COST_GROUP_ID, String(cooldownSecondsRemaining), this.getCostColor(ActionCostType.Default));
        }
    }

    private setCostGroupContent(groupId: string, text: string, color: CostColor): void {
        this.setVisible(groupId, true);

        const textEl = this.ids.get(`${groupId}Text`);
        if (textEl) {
            textEl.textContent = text;
            textEl.setAttribute("fill", color.base);
            textEl.setAttribute("stroke", color.glow);
        }
    }

    private getCostColor(type: ActionCostType): CostColor {
        return COST_TYPE_COLORS[type] ?? COST_TYPE_COLORS[ActionCostType.Default];
    }

    setChargeCount(count: number | undefined): void {
        this.setVisible(CHARGE_COUNT_ZERO_GROUP_ID, count === 0);
        this.setVisible(CHARGE_COUNT_GROUP_ID, count != null && count > 0);
        if (count == null || count <= 0) return;

        const text = String(count);
        const shadow = this.ids.get(CHARGE_COUNT_SHADOW_ID);
        if (shadow) shadow.textContent = text;

        const crisp = this.ids.get(CHARGE_COUNT_TEXT_ID);
        if (crisp) crisp.textContent = text;
    }

    setRechargeTimerDonut(progressPercent: number | undefined): void {
        const active = progressPercent != null && progressPercent > 0 && progressPercent < 100;
        this.setVisible(RECHARGE_TIMER_DONUT_GROUP_ID, active);
        if (!active) return;

        const offset = RECHARGE_TIMER_DONUT_CIRCUMFERENCE * (1 - progressPercent! / 100);
        const group = this.ids.get(RECHARGE_TIMER_DONUT_GROUP_ID);
        const circles = group?.getElementsByTagName("circle") ?? [];
        for (let i = 0; i < circles.length; i++) {
            circles[i].setAttribute("stroke-dashoffset", offset.toFixed(3));
        }
    }

    setTypeIcon(svg: string | undefined): void {
        const group = this.ids.get(TYPE_ICON_ID);
        this.setVisible(TYPE_ICON_ID, svg != null);
        if (!group || svg == null) return;

        while (group.firstChild) {
            group.removeChild(group.firstChild);
        }

        const fragment = new DOMParser().parseFromString(
            `<g xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink">${svg}</g>`,
            "image/svg+xml",
        );

        const root = fragment.documentElement;
        while (root?.firstChild) {
            group.appendChild(root.firstChild);
        }
    }

    setVisible(id: string, visible: boolean): void {
        const el = this.ids.get(id);
        el?.setAttribute("display", visible ? "inline" : "none");
    }

    private indexById(doc: Document): Map<string, Element> {
        const ids = new Map<string, Element>();
        const all = doc.getElementsByTagName("*");
        for (let i = 0; i < all.length; i++) {
            const id = all[i].getAttribute("id");
            if (id) ids.set(id, all[i]);
        }
        return ids;
    }
}

function toSvgDataUrl(svg: string): string {
    return `data:image/svg+xml;charset=utf8,${svg.replaceAll("%", "%25").replaceAll("#", "%23")}`;
}

export const hotbarSlotRenderer = new HotbarSlotRenderer();
