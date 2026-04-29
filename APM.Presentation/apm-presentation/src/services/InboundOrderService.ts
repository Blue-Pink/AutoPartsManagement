import type { InboundOrder, InboundItem } from '@/interfaces/DTOEntities'
import $ from '@/utils/requestor'
import type { UsualApiData } from '@/interfaces/HttpReponse'

class InboundOrderService {
    GetInboundOrders(pageIndex: number, pageSize: number, sortField?: string, sortDesc: boolean = false): Promise<UsualApiData<InboundOrder>> {
        let url = `InboundOrder/GetInboundOrders?pageIndex=${pageIndex}&pageSize=${pageSize}`
        if (sortField) url += `&sortField=${encodeURIComponent(sortField)}`
        url += `&sortDesc=${sortDesc}`
        return $.get(url)
    }

    EditInboundOrder(order: InboundOrder | undefined): Promise<UsualApiData<InboundOrder>> {
        if (order) return $.post('InboundOrder/EditInboundOrder', order)
        throw new Error('入库单数据不可为空')
    }

    GetInboundItems(orderId: string): Promise<UsualApiData<InboundItem>> {
        return $.get(`InboundOrder/GetInboundItems?orderId=${encodeURIComponent(orderId)}`)
    }

    EditInboundItem(item: InboundItem | undefined): Promise<UsualApiData<InboundItem>> {
        if (item) return $.post('InboundOrder/EditInboundItem', item)
        throw new Error('入库明细数据不可为空')
    }

}

export default new InboundOrderService()
