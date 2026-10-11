import {DOMParser, XMLSerializer} from "@xmldom/xmldom";
import type {Document, Element} from "@xmldom/xmldom";
import {fastClone} from "#/util/DomUtil";
import {ActionCostType} from "#/client/rpc/messages/ActionAppearance";

import slotTemplateSvg from "../../../assets/templates/HotbarSlot.svg";

export interface HotbarSlotCost {
    text: string;
    type: ActionCostType;
    position: "left" | "right";
}

// Element IDs in assets/templates/HotbarSlot.svg.
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

interface ICostColor {
    base: string;
    glow: string;
}

const COST_TYPE_COLORS: Record<ActionCostType, ICostColor> = {
    [ActionCostType.Default]: {base: "#ffffff", glow: "#333333"},   // White
    [ActionCostType.Health]: {base: "#c9ffe4", glow: "#0a5f24"},    // Green (HP)
    [ActionCostType.Magic]: {base: "#ffd9fa", glow: "#985090"},     // Light Pink (MP)
    [ActionCostType.Tactical]: {base: "#ffe6bb", glow: "#7c1d00"},  // Orange (TP?)
    [ActionCostType.Crafting]: {base: "#e5c4ff", glow: "#b71b7e"},  // Pink (DoH - CP)
    [ActionCostType.Gathering]: {base: "#fff1d4", glow: "#7f7c1d"}, // Yellow (DoL - GP)
    [ActionCostType.JobGauge]: {base: "#dcffff", glow: "#3040ec"},  // Blue (Job Gauge)
    [ActionCostType.Ceruleum]: {base: "#fdffd9", glow: "#958a12"},  // Bright Yellow (Rival Wings - CE)
};

/**
 * One render's copy of the hotbar slot SVG template. Owns everything about the template's structure (element IDs,
 * geometry, colors), so callers only say *what* to show.
 */
export class HotbarSlotDocument {
    // cache the template across invocations for performance.
    // static exception: the template is an embedded asset, so it can't be changed.
    private static _template: Document | undefined;

    private readonly _document: Document;
    private readonly _ids: ReadonlyMap<string, Element>;

    public constructor() {
        this._document = fastClone(HotbarSlotDocument.template());
        this._ids = HotbarSlotDocument.indexById(this._document);
    }

    public setIcon(iconBase64: string): void {
        this._ids.get(ICON_ID)?.setAttribute("xlink:href", `data:image/png;base64,${iconBase64}`);
    }

    public setHighlighted(highlighted: boolean): void {
        this.setVisible(HIGHLIGHT_GROUP_ID, highlighted);
    }

    public setRecastTimer(elapsedPercent: number | undefined): void {
        const active = elapsedPercent != null && elapsedPercent > 0 && elapsedPercent < 100;
        this.setVisible(RECAST_TIMER_GROUP_ID, active);
        if (!active) return;

        const maskPath = HotbarSlotDocument.buildWedgeMaskPath(elapsedPercent);
        this._ids.get(RECAST_TIMER_MASK_PATH_ID)?.setAttribute("d", maskPath);

        const wedgePath = HotbarSlotDocument.buildWedgePath(elapsedPercent, COOLDOWN_CIRCLE_RADIUS);
        this._ids.get(RECAST_TIMER_WEDGE_MASK_PATH_ID)?.setAttribute("d", wedgePath);
        this._ids.get(RECAST_TIMER_EDGE_GLOW_ID)?.setAttribute("d", wedgePath);
        this._ids.get(RECAST_TIMER_EDGE_CRISP_ID)?.setAttribute("d", wedgePath);
    }

    public setCost(cost: HotbarSlotCost | undefined): void {
        this.setVisible(LEFT_COST_GROUP_ID, false);
        this.setVisible(RIGHT_COST_GROUP_ID, false);

        if (cost) {
            const groupId = cost.position === "left" ? LEFT_COST_GROUP_ID : RIGHT_COST_GROUP_ID;
            const color = COST_TYPE_COLORS[cost.type] ?? COST_TYPE_COLORS[ActionCostType.Default];
            this.setCostGroupContent(groupId, cost.text, color);
        }
    }

    public setChargeCount(count: number | undefined): void {
        this.setVisible(CHARGE_COUNT_ZERO_GROUP_ID, count === 0);
        this.setVisible(CHARGE_COUNT_GROUP_ID, count != null && count > 0);
        if (count == null || count <= 0) return;

        const text = String(count);
        const shadow = this._ids.get(CHARGE_COUNT_SHADOW_ID);
        if (shadow) shadow.textContent = text;

        const crisp = this._ids.get(CHARGE_COUNT_TEXT_ID);
        if (crisp) crisp.textContent = text;
    }

    public setRechargeTimerDonut(progressPercent: number | undefined): void {
        const active = progressPercent != null && progressPercent > 0 && progressPercent < 100;
        this.setVisible(RECHARGE_TIMER_DONUT_GROUP_ID, active);
        if (!active) return;

        const offset = RECHARGE_TIMER_DONUT_CIRCUMFERENCE * (1 - progressPercent / 100);
        const circles = this._ids.get(RECHARGE_TIMER_DONUT_GROUP_ID)?.getElementsByTagName("circle") ?? [];
        for (let i = 0; i < circles.length; i++) {
            circles[i].setAttribute("stroke-dashoffset", offset.toFixed(3));
        }
    }

    public setRechargeTimerSweep(progressPercent: number | undefined): void {
        const active = progressPercent != null && progressPercent > 0 && progressPercent < 100;
        this.setVisible(RECHARGE_TIMER_SWEEP_GROUP_ID, active);
        if (!active) return;

        const fill = this._ids.get(RECHARGE_TIMER_SWEEP_FILL_ID);
        fill?.setAttribute("d", HotbarSlotDocument.buildWedgePath(progressPercent, RECHARGE_TIMER_SWEEP_RADIUS));

        const sweepDeg = (progressPercent / 100) * 360;
        const edgeGroup = this._ids.get(RECHARGE_TIMER_SWEEP_EDGE_GROUP_ID);
        edgeGroup?.setAttribute("transform", `rotate(${sweepDeg.toFixed(2)} ${COOLDOWN_CENTER} ${COOLDOWN_CENTER})`);
    }

    /** Replaces the type badge's contents with `svg` (markup for the badge's group), or hides it. */
    public setTypeIcon(svg: string | undefined): void {
        const group = this._ids.get(TYPE_ICON_ID);
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

    /** The finished image, as a data URL ready for `setImage`. */
    public toDataUrl(): string {
        const svg = new XMLSerializer().serializeToString(this._document);
        return `data:image/svg+xml;charset=utf8,${svg.replaceAll("%", "%25").replaceAll("#", "%23")}`;
    }

    private setVisible(id: string, visible: boolean): void {
        this._ids.get(id)?.setAttribute("display", visible ? "inline" : "none");
    }

    private setCostGroupContent(groupId: string, text: string, color: ICostColor): void {
        this.setVisible(groupId, true);

        const textEl = this._ids.get(`${groupId}Text`);
        if (textEl) {
            textEl.textContent = text;
            textEl.setAttribute("fill", color.base);
            textEl.setAttribute("stroke", color.glow);
        }
    }

    private static template(): Document {
        HotbarSlotDocument._template ??= new DOMParser().parseFromString(slotTemplateSvg, "image/svg+xml");
        return HotbarSlotDocument._template;
    }

    private static buildWedgeMaskPath(elapsedPercent: number): string {
        const square = `M0,0 H80 V80 H0 Z`;
        const wedge = HotbarSlotDocument.buildWedgePath(elapsedPercent, COOLDOWN_CIRCLE_RADIUS);

        return `${square} ${wedge}`;
    }

    private static buildWedgePath(elapsedPercent: number, r: number): string {
        const cx = COOLDOWN_CENTER;
        const cy = COOLDOWN_CENTER;

        const sweepDeg = (elapsedPercent / 100) * 360;
        const largeArc = sweepDeg > 180 ? 1 : 0;
        const start = HotbarSlotDocument.polarPoint(cx, cy, r, 0);
        const end = HotbarSlotDocument.polarPoint(cx, cy, r, sweepDeg);

        return `M${cx},${cy} L${start.x},${start.y} A${r},${r} 0 ${largeArc},1 ${end.x},${end.y} Z`;
    }

    private static polarPoint(cx: number, cy: number, r: number, angleDeg: number): { x: number; y: number } {
        const rad = (Math.PI / 180) * angleDeg;
        return {x: cx + r * Math.sin(rad), y: cy - r * Math.cos(rad)};
    }

    private static indexById(doc: Document): Map<string, Element> {
        const ids = new Map<string, Element>();
        const all = doc.getElementsByTagName("*");
        for (let i = 0; i < all.length; i++) {
            const id = all[i].getAttribute("id");
            if (id) ids.set(id, all[i]);
        }
        return ids;
    }
}
