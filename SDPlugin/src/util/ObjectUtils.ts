export class ObjectUtils {
    static isObject(item: any): boolean {
        return item !== null && typeof item === 'object' && !Array.isArray(item);
    }

    static deepMerge<T extends object>(target: T, ...sources: Array<Partial<T>>): T {
        if (!sources.length) return target;
        const source = sources.shift();

        if (ObjectUtils.isObject(target) && ObjectUtils.isObject(source)) {
            for (const key in source) {
                if (Object.prototype.hasOwnProperty.call(source, key)) {
                    const sourceValue = source[key];
                    if (ObjectUtils.isObject(sourceValue) && ObjectUtils.isObject(target[key])) {
                        target[key] = ObjectUtils.deepMerge(target[key] as any, sourceValue as any);
                    } else {
                        (target as any)[key] = sourceValue;
                    }
                }
            }
        }
        return ObjectUtils.deepMerge(target, ...sources);
    }
}

