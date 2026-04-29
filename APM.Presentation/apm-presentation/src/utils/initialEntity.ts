import type { BaseEntity, InboundItem, InboundOrder, Part, Supplier, User } from "../interfaces/DTOEntities";

/**
 * 创建一个继承自 BaseEntity 的默认对象，可传入泛型以获得精确类型提示。
 * 可通过 overrides 覆盖默认字段。
 */
export function createDefaultEntity<T extends BaseEntity>(overrides?: Partial<T>): T {
    const base: BaseEntity = {
        id: "00000000-0000-0000-0000-000000000000",
        createdAt: "2026-04-29 12:23:04",
        modifiedAt: "2026-04-29 12:23:04",
    };
    return { ...base, ...(overrides || {}) } as T;
}

export const _initialPart: Part = {
    ...createDefaultEntity<Part>({
        model: null,
        brand: null,
        categoryId: null,
        unitId: null,
        costPrice: 0,
        sellingPrice: 0,
        minStock: 0,
        maxStock: 0,
        remark: null,
        partName: null,
        oeCode: null,
        categoryName: null,
        unitName: null,
    })
}

export const _initialSupplier = {
    ...createDefaultEntity<Supplier>({
        name: null,
        contact: null,
        phone: null,
        address: null,
    })
}

export const _initialUser = {
    ...createDefaultEntity<User>({
        username: null,
        realname: null,
        password: null,
        roles: null,
        confirmPassword: null,
        isActive: true,
    })
}

export const _initialInboundOrder = {
    ...createDefaultEntity<InboundOrder>({
        orderNo: null,
        supplierId: null,
        totalAmount: 0,
        operatorUserId: null,
        remark: null,
    })
}

export const _initialInboundItem = {
    ...createDefaultEntity<InboundItem>({
        inboundOrderId: null,
        partId: null,
        quantity: 0,
        price: 0,
        totalAmount: 0,
    })
}