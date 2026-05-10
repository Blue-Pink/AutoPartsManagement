import $ from '@/utils/requestor';
import type { UsualApiData } from '@/interfaces/HttpReponse';
import { ConstDictionary } from '@/utils/const-dictionary';
import type { BaseEntity } from '@/interfaces/Entities';

class UsualEntityService {
    Get<T>(entityName: string, id: string): Promise<UsualApiData<T>> {
        if (entityName && id && id !== ConstDictionary.EMPTY_GUID) {
            return $.get(`Entity/Get/${entityName}/${id}`);
        }
        throw new Error(`Get 需要有效的 entityName,id 参数，不能是空或全零 GUID`);
    }

    AutoNumber(entityName: string, prefix: string, digits: number = 4): Promise<UsualApiData<string>> {
        if (entityName && prefix && digits > 0) {
            return $.get(`Entity/AutoNumber?entityName=${encodeURIComponent(entityName)}&prefix=${encodeURIComponent(prefix)}&digits=${digits}`);
        }
        throw new Error(`AutoNumber 需要有效的 entityName, prefix, digits 参数，不能是空或无效值`);
    }

    Delete(entityName: string, ids: string[]): Promise<UsualApiData<number>> {
        if (entityName && ids && ids.length > 0) {
            return $.delete(`Entity/Delete/${entityName}`, { data: ids });
        }
        throw new Error(`Delete 需要有效的 entityName 和 ids 参数，ids 不能为空数组`);
    }

    Edit<T>(entityName: string, entity: BaseEntity): Promise<UsualApiData<T>> {
        if (entityName && entity) {
            return $.post(`Entity/Edit/${entityName}`, entity);
        }
        throw new Error(`Edit 需要有效的 entityName 和 entity 参数`);
    }

    GetDataSet<T>(entityName: string, pageIndex: number = 1, pageSize: number = 10, orderBy: string = "", descending: boolean = false, filter: string = "", depth: number = 1): Promise<UsualApiData<T>> {
        if (entityName) {
            let url = `Entity/GetDataSet/${encodeURIComponent(entityName)}?pageIndex=${pageIndex}&pageSize=${pageSize}&depth=${depth}`;
            if (orderBy) url += `&orderBy=${encodeURIComponent(orderBy)}`;
            url += `&descending=${descending}`;
            if (filter) url += `&filter=${encodeURIComponent(filter)}`;
            return $.get(url);
        }
        throw new Error(`GetDataSet 需要有效的 entityName 参数`);
    }


    GetChildrenDataSet<T>(parentEntityName: string, childEntityName: string, parentId: string, pageIndex: number = 1, pageSize: number = 10, orderBy: string = "", descending: boolean = false, filter: string = "", depth: number = 1): Promise<UsualApiData<T>> {
        if (parentEntityName && childEntityName && parentId && parentId !== ConstDictionary.EMPTY_GUID) {

            let url = `Entity/GetChildrenDataSet/${encodeURIComponent(parentEntityName)}/${encodeURIComponent(childEntityName)}/${encodeURIComponent(parentId)}?pageIndex=${pageIndex}&pageSize=${pageSize}&depth=${depth}`;
            if (orderBy) url += `&orderBy=${encodeURIComponent(orderBy)}`;
            url += `&descending=${descending}`;
            if (filter) url += `&filter=${encodeURIComponent(filter)}`;
            return $.get(url);
        }
        throw new Error(`GetChildrenDataSet 需要有效的 parentEntityName, childEntityName, parentId 参数`);
    }
}

export default new UsualEntityService();