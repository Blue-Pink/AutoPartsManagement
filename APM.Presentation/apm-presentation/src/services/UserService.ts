import type { Role, User } from '@/interfaces/DTOEntities'
import $ from '@/utils/requestor'
import type { UsualApiData } from '@/interfaces/HttpReponse'

class UserService {
    UserLogin(user: User): Promise<UsualApiData<string>> {
        return $.post(`User/UserLogin`, user);
    }

    CheckUserToken(): Promise<UsualApiData<boolean>> {
        return $.get('User/CheckUserToken')
    }

    GetCurrentUser(): Promise<UsualApiData<User>> {
        return $.get('User/GetCurrentUser');
    }

    GetUsers(pageIndex: number, pageSize: number, sortField?: string, sortDesc: boolean = false, search?: string): Promise<UsualApiData<User>> {
        let url = `User/GetUsers?pageIndex=${pageIndex}&pageSize=${pageSize}`
        if (sortField) url += `&sortField=${encodeURIComponent(sortField)}`
        url += `&sortDesc=${sortDesc}`
        if (search) url += `&search=${encodeURIComponent(search)}`
        return $.get(url)
    }

    GetRoles(pageIndex: number, pageSize: number, sortField?: string, sortDesc: boolean = false, search?: string): Promise<UsualApiData<Role>> {
        let url = `User/GetRoles?pageIndex=${pageIndex}&pageSize=${pageSize}`
        if (sortField) url += `&sortField=${encodeURIComponent(sortField)}`
        url += `&sortDesc=${sortDesc}`
        if (search) url += `&search=${encodeURIComponent(search)}`
        return $.get(url)
    }

    EditUser(user: User | undefined): Promise<UsualApiData<User>> {
        if (user) return $.post('User/EditUser', user)
        throw new Error('用户数据不可为空')
    }
}

export default new UserService()
