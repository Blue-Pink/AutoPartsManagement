import $ from '@/utils/requestor';
import type { UsualApiData } from '@/interfaces/HttpReponse';

class UsualEntityService {
    Get<T>(entityName: string, id: string): Promise<UsualApiData<T>> {
        return $.get(`Entity/Get/${entityName}/${id}`);
    }

    AutoNumber(entityName: string, prefix: string, digits: number = 4): Promise<UsualApiData<string>> {
        return $.get(`Entity/AutoNumber?entityName=${encodeURIComponent(entityName)}&prefix=${encodeURIComponent(prefix)}&digits=${digits}`);
    }

    Delete(entityName: string, ids: string[]): Promise<UsualApiData<number>> {
        return $.delete(`Entity/Delete/${entityName}`, { data: ids });
    }

    Create(entityName: string, entity: any): Promise<UsualApiData<any>> {
        return $.post(`Entity/Create/${entityName}`, entity);
    }

    GetChildrenDataSetQuery(parentEntityName: string, childEntityName: string, parentId: string, pageIndex?: number, pageSize?: number, sortField?: string, sortDesc: boolean = false): Promise<UsualApiData<any>> {
        let url = `Entity/GetChildrenDataSetQuery/${encodeURIComponent(parentEntityName)}/${encodeURIComponent(childEntityName)}/${encodeURIComponent(parentId)}?pageIndex=${pageIndex}&pageSize=${pageSize}`;
        if (sortField) url += `&sortField=${encodeURIComponent(sortField)}`;
        url += `&sortDesc=${sortDesc}`;
        return $.get(url);
    }
}

export default new UsualEntityService();