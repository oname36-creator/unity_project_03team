// practicev2.cpp : 이 파일에는 'main' 함수가 포함됩니다. 거기서 프로그램 실행이 시작되고 종료됩니다.
//
#include<map>
#include <iostream>
#include<vector>
#include <algorithm>
#include <queue>

using namespace std;


struct family
{
	string rank;
	int id;


};
//1번
vector<vector<family>>fam(11);
vector<bool> visited(11,false);
vector<vector<long long>> stair;

//3번
vector<vector<int>>computer(11);
vector<bool> visited2(11, false);

//int stairs(int stairsNum);
void dfs(int node);
long long dqarray(int number);
int ComputerCount(int number);
int solution(vector<int> numbers, int target);
int get_node_count(int n, const vector<vector<int>>& adj, int start, int cut_v1, int cut_v2);
int solutionBFS(int n, vector<vector<int>> wires);
int solutionDij(int N, vector<vector<int> > road, int K);
int solutionDij(int N, vector<vector<int>> road, int K);

int main()
{	
	//1번
	fam[0].push_back({ "할아버지",1});
	fam[1].push_back({ "아버지",2});
	fam[1].push_back({ "작은아버지",3});

	fam[5].push_back({ "조카",7});
	fam[2].push_back({ "나",4});
	fam[2].push_back({ "동생",5});
	fam[3].push_back({ "사촌",6});
	fam[4].push_back({ "딸",8});
	fam[8].push_back({ "손녀",9});

	for (int i = 0; i < fam.size(); i++) {
		sort(fam[i].begin(), fam[i].end(), [](const family& a, const family& b) {
			return a.id < b.id;
			});
	}

	cout << "== 패밀리 =="<<endl;
	
	dfs(0);

	//2번



	cout << dqarray(10) << endl;
	
	

	// dfs/bfs 문제 1
	

	computer[1].push_back(2);
	computer[1].push_back(5);
	computer[2].push_back(1);
	computer[2].push_back(5);
	computer[3].push_back(2);
	computer[5].push_back(1);
	computer[5].push_back(2);
	computer[6].push_back(5);
	computer[4].push_back(7);
	computer[7].push_back(4);


	int Ccount = ComputerCount(1);

	cout << "출력 값 : " << Ccount << endl;

	//dfs 문제 2
	cout << solution({ 1,1,1,1,1 }, 3) << endl;


	//bfs 문제 3
	int N = 9;
	vector<vector<int>> wires = { {1, 3}, {2, 3}, {3, 4}, {4, 5}, {4, 6}, {4, 7}, {7, 8}, {7, 9} };

	// 결과 값 출력
	int answer = solutionBFS(N, wires);

	cout << answer << endl;
	// -> answer 값이 "3" 이 나오면 됩니다.


	// 다익스트라 문제 1번 배달

	int N2 = 5;  // 5개의 마을
	int K = 3;  // 3시간 이하
	vector<vector<int>> road = { {1, 2, 1}, {2, 3, 3}, {5, 2, 2}, {1, 4, 2}, {5, 3, 1}, {5,4,2} };

	// 결과 값 출력
	int answer2 = solutionDij(N2,road,K);

	// -> answer 값이 "4" 이 나오면 됩니다.

	cout << answer2 << endl;
	return 0;

	// 경주로는 포기하겠습니다...


}


//1번 함수
void dfs(int gener) 
{
	static int gen= 0;
	++gen;
	visited[gener] = true;
	
	for (const auto& next : fam[gener]) {
		int next_id = next.id;
	
		if (!visited[next_id]) {


			for (int i = 1; i < gen; ++i)
			{
				cout << "  ";
			}
			if (gen != 1)
			{
				cout << "ㄴ";
			}
			
			cout << next.rank << " " << gen << "세대" << endl;

			dfs(next_id);
		}
			--gen;
			
	}
}




long long dqarray(int number)
{
	if (number == 0)return 1;
	if (number == 1)return 1;
	if (number == 2)return 2;

	vector<vector<long long>> dp(number + 1, vector<long long>(3, 0));

	dp[1][1] = 1; 
	dp[2][0] = 1;
	dp[2][2] = 1;

	for (int i = 3; i <= number; ++i)
	{
		dp[i][1] = dp[i - 1][0]; 
		dp[i][2] = dp[i - 1][1];

		long long from_i_minus_2 = dp[i - 2][0] + dp[i - 2][1] + dp[i - 2][2];

		
		long long from_i_minus_3 = dp[i - 3][0] + dp[i - 3][1] + dp[i - 3][2];

		
		dp[i][0] = from_i_minus_2 + from_i_minus_3;
	}

	return dp[number][0] + dp[number][1] + dp[number][2];
}

int ComputerCount(int number)
{
	visited2[number] = true;
	static int countt = 1;
	++countt;
	for (const auto& next : computer[number])
	{
		int next_co = next;

		if (!visited2[next_co])
		{
			ComputerCount(next_co);
		}
	}
	return countt;
}

int solution(vector<int> numbers, int target) {

	if (numbers.empty())// 타겟 넘버
	{
		return(target == 0) ? 1 : 0;
	}

	int last_num = numbers.back();
	numbers.pop_back();


	int plus = solution(numbers, target - last_num);
	int minus = solution(numbers, target + last_num);

	return plus + minus;
}


int solutionBFS(int n, vector<vector<int>> wires) {
	int answer = n;

	
	vector<vector<int>> adj(n + 1);
	for (const auto& wire : wires) {
		int u = wire[0];
		int v = wire[1];
		adj[u].push_back(v);
		adj[v].push_back(u);
	}

	
	for (const auto& wire : wires) {
		int cut_v1 = wire[0];
		int cut_v2 = wire[1];

		
		int cnt1 = get_node_count(n, adj, cut_v1, cut_v1, cut_v2);
		int cnt2 = n - cnt1; 

		answer = min(answer, abs(cnt1 - cnt2));
	}

	return answer;
}

int get_node_count(int n, const vector<vector<int>>& adj, int start, int cut_v1, int cut_v2) {

	vector<bool> visited(n + 1, false);
	queue<int> q;

	q.push(start);
	visited[start] = true;
	int count = 1;

	while (!q.empty()) {
		int curr = q.front();
		q.pop();

		for (int next : adj[curr]) {
			
			if ((curr == cut_v1 && next == cut_v2) || (curr == cut_v2 && next == cut_v1)) {
				continue;
			}

			if (!visited[next]) {
				visited[next] = true;
				q.push(next);
				count++;
			}
		}
	}
	return count;
}



int solutionDij(int N, vector<vector<int>> road, int K)
{
	struct Pos
	{
		int vil;
		int n_vil;
		int cost;
		bool operator<(const Pos& other) const
		{
			return cost > other.cost;
		}
	};
	vector<vector<pair<int, int>>> graph(N + 1);

	
	vector<int> best(N + 1, 1e9);
	priority_queue<Pos> PQ;

	for (auto a : road)
	{
		int village = a[0];
		int n_village = a[1];
		int v_cost = a[2];

		graph[village].push_back({ n_village, v_cost });
		graph[n_village].push_back({ village, v_cost });
	}
	best[1] = 0;

	
	PQ.push({ 1, 1, 0 });

	while (PQ.empty() == false)
	{
		Pos curr = PQ.top();
		PQ.pop();

		int cost = curr.cost;
		if (curr.cost > best[curr.vil]) continue;
		if (curr.cost > K) continue;

		for (auto a : graph[curr.vil])
		{
			int next_vil = a.first;
			int next_cost = curr.cost + a.second;

			if (next_cost < best[next_vil])
			{
				best[next_vil] = next_cost;
				
				PQ.push({ next_vil, 0, next_cost });
			}
		}
	}

	int answer = 0;
	for (int i = 1; i <= N; ++i)
	{
		if (best[i] <= K)
			++answer;
	}

	return answer;
}

//int stairs( int stairsNum)
//{
//	
//	static int sum = 0;
//	static int count = 0;
//	if (stairsNum < count)
//	{
//		int result = sum;
//		count = 0;
//		sum = 0;
//		return result;
//	}
//	int a = 1;
//	int b = 1;
//	int c = 1;
//	for (int i = 1; stairsNum + 1 > i; ++i)
//	{
//		c *= i;
//	}
//	for (int j = 1; stairsNum - count+1 > j; ++j)
//	{
//		b *= j;
//	}
//	if (count != 0)
//	{
//		for (int k = 1; count+1 > k; ++k)
//		{
//			a *= k;
//		}
//	}
//	sum += (c/(b*a));
//	++count;
//	stairs(stairsNum - 1);
//
//}