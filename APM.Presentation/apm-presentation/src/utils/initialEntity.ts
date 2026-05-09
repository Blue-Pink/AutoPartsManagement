import type { BaseEntity, Customer, InboundItem, InboundOrder, Part, Supplier, User, OutboundOrder, OutboundItem } from "../interfaces/Entities";
import { ConstDictionary } from "./const-dictionary";
/**
 * 创建一个继承自 BaseEntity 的默认对象，可传入泛型以获得精确类型提示。
 * 可通过 overrides 覆盖默认字段。
 */
export function createDefaultEntity<T extends BaseEntity>(overrides?: Partial<T>): T {
    const base: BaseEntity = {
        id: ConstDictionary.EMPTY_GUID,
        createdAt: ConstDictionary.CURRENT_DATETIME,
        modifiedAt: ConstDictionary.CURRENT_DATETIME,
        operatorUserId: ConstDictionary.EMPTY_GUID,
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
        category: null,
        unit: null,
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

export const _initialCustomer = {
    ...createDefaultEntity<Customer>({
        name: null,
        contactPerson: null,
        phone: null,
        address: null,
        remark: null,
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
        inboundDate: ConstDictionary.CURRENT_DATETIME,
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

export const _initialOutboundOrder = {
    ...createDefaultEntity<OutboundOrder>({
        orderNo: null,
        totalAmount: 0,
        remark: null,
        customerId: null,
        customer: null,
        outboundItems: null,
        outboundDate: ConstDictionary.CURRENT_DATETIME,
    })
}

export const _initialOutboundItem = {
    ...createDefaultEntity<OutboundItem>({
        outboundOrderId: null,
        outboundOrder: null,
        partId: null,
        part: null,
        quantity: 0,
        price: 0,
        totalAmount: 0,
    })
}
